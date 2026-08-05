# Arquitectura de UrbanSync

## 1. Propósito

Este documento describe la organización técnica de UrbanSync y las reglas principales que deben respetarse al incorporar nuevos módulos.

UrbanSync utiliza un monorepo que contiene:

- API REST en ASP.NET Core.
- Aplicación Web MVC.
- Aplicación móvil Flutter.
- Pruebas automatizadas.
- Scripts de base de datos.
- Configuración Docker.
- Integración continua mediante GitHub Actions.

La arquitectura busca facilitar:

- Separación de responsabilidades.
- Sustitución de implementaciones.
- Pruebas automatizadas.
- Desarrollo paralelo.
- Mantenimiento por módulos.
- Ejecución independiente del IDE.

---

## 2. Vista general

```text
┌────────────────────┐
│   Flutter Mobile   │
└─────────┬──────────┘
          │
          │ HTTP / JSON / Bearer JWT
          │
┌─────────▼──────────┐
│ ASP.NET Core API   │
└─────────┬──────────┘
          │
          │ SQL
          │
┌─────────▼──────────┐
│ Azure SQL / SQL    │
└────────────────────┘

┌────────────────────┐
│ ASP.NET Core MVC   │
└─────────┬──────────┘
          │
          │ HTTP / JSON / Bearer JWT
          └──────────────► ASP.NET Core API
```

La Web MVC y Flutter no deben acceder directamente a la base de datos.

Toda operación persistente debe realizarse mediante la API.

---

## 3. Backend

El backend se encuentra en:

```text
src/backend/
```

Está dividido en cuatro proyectos.

### 3.1 UrbanSync.Domain

Ruta:

```text
src/backend/UrbanSync.Domain/
```

Responsabilidades:

- Entidades del negocio.
- Conceptos centrales.
- Reglas que no dependen de infraestructura.
- Tipos compartidos del dominio.

No debe depender de:

- ASP.NET Core.
- SQL Server.
- DTO HTTP.
- Controladores.
- Flutter.
- Web MVC.
- Implementaciones de infraestructura.

---

### 3.2 UrbanSync.Application

Ruta:

```text
src/backend/UrbanSync.Application/
```

Responsabilidades:

- Casos de uso.
- Servicios de aplicación.
- DTO internos.
- Interfaces de persistencia.
- Interfaces de autenticación.
- Coordinación entre dominio y repositorios.

Puede depender de:

```text
UrbanSync.Domain
```

No debe depender directamente de:

```text
UrbanSync.Infrastructure
UrbanSync.Api
```

Las interfaces se colocan en Application y las implementaciones se colocan en Infrastructure.

Ejemplo:

```text
Application
└── IUsuarioRepository

Infrastructure
└── UsuarioRepository
```

---

### 3.3 UrbanSync.Infrastructure

Ruta:

```text
src/backend/UrbanSync.Infrastructure/
```

Responsabilidades:

- Implementaciones SQL.
- Repositorios.
- Fábricas de conexión.
- Hashing de contraseñas.
- Generación de JWT.
- Integraciones externas.

Puede depender de:

```text
UrbanSync.Application
UrbanSync.Domain
```

Registra sus dependencias mediante:

```csharp
services.AddInfrastructure(configuration);
```

---

### 3.4 UrbanSync.Api

Ruta:

```text
src/backend/UrbanSync.Api/
```

Responsabilidades:

- Controladores.
- Contratos HTTP.
- Configuración JWT.
- Autorización.
- Swagger.
- CORS.
- Middleware.
- Health checks.
- Composición de dependencias.

La API es el punto de entrada del backend.

Su `Program.cs` debe mantenerse pequeño y delegar la configuración a extensiones.

---

## 4. Dependencias permitidas

```text
Domain
  ↑
Application
  ↑
Infrastructure

Application ─────► Domain
Infrastructure ──► Application + Domain
Api ─────────────► Application + Infrastructure
```

No se permiten referencias circulares.

No se permite:

```text
Application ──► Infrastructure
Domain ───────► Application
Domain ───────► Api
Infrastructure ──► Api
```

---

## 5. Web MVC

Ruta:

```text
src/web/UrbanSync.Web/
```

La Web se organiza principalmente mediante:

```text
ApiClients/
Authentication/
Controllers/
Extensions/
Presentation/
Services/
ViewModels/
Views/
wwwroot/
```

### ApiClients

Contienen clientes HTTP especializados por módulo:

```text
Authentication
Users
Roles
Incidents
Reports
WorkOrders
```

Cada cliente debe depender de `HttpClient` y representar un único área funcional.

No debe volver a crearse un cliente monolítico que maneje todos los endpoints.

### Presentation

Los servicios de presentación construyen los modelos que consumen las vistas.

Ejemplo:

```text
Presentation/Dashboard/DashboardPageService
Presentation/Users/UserManagementPageService
```

Los controladores deben concentrarse en:

- Recibir solicitudes.
- Validar modelos.
- Aplicar autorización.
- Invocar servicios.
- Seleccionar vistas o redirecciones.

No deben contener consultas complejas, agrupaciones extensas ni construcción pesada de páginas.

### Authentication

La Web utiliza cookies para su sesión local.

El JWT recibido desde la API se almacena como claim y se agrega automáticamente a las solicitudes salientes mediante un `DelegatingHandler`.

```text
Cookie de la Web
      │
      └── claim access_token
                │
                └── Authorization: Bearer <token>
```

La Web no firma ni valida tokens JWT. Esa responsabilidad corresponde a la API.

---

## 6. Flutter

Ruta:

```text
src/mobile/
```

La aplicación se organiza por funcionalidades:

```text
lib/
├── app/
├── core/
├── features/
└── shared/
```

### app

Configuración general:

- Router.
- Tema.
- Aplicación raíz.

### core

Infraestructura común:

- Configuración de ambientes.
- Cliente Dio.
- Almacenamiento seguro.
- Manejo de errores.

### features

Cada funcionalidad mantiene juntos sus componentes de datos, dominio y presentación.

Ejemplo:

```text
features/auth/
├── data/
├── domain/
└── presentation/
```

### shared

Contiene componentes reutilizables que no pertenecen a una sola funcionalidad.

La aplicación móvil almacena la sesión mediante `flutter_secure_storage` y agrega el JWT automáticamente mediante un interceptor de Dio.

---

## 7. Autenticación

### Flujo

```text
Cliente
  │
  ├── POST /api/auth/login
  │
  ▼
API valida usuario y contraseña
  │
  ▼
API genera JWT firmado
  │
  ▼
Cliente almacena token y expiración
  │
  ▼
Authorization: Bearer <token>
```

El token contiene como mínimo:

- Identificador del usuario.
- Nombre.
- Correo.
- Rol.
- Identificador único del token.
- Expiración.

La API utiliza autorización basada en roles.

Ejemplo:

```csharp
[Authorize(Roles = "Administrador")]
```

---

## 8. Persistencia

Infrastructure utiliza una fábrica de conexiones SQL:

```text
IDbConnectionFactory
SqlConnectionFactory
```

Los repositorios reciben la abstracción y no deben crear cadenas de conexión manualmente.

La cadena se configura mediante:

```text
ConnectionStrings:UrbanSyncDb
```

La documentación definitiva de scripts, migraciones, índices y seeds se completará cuando terminen los módulos funcionales pendientes.

---

## 9. Pruebas

Las pruebas se encuentran en:

```text
tests/backend/
```

Proyectos:

```text
UrbanSync.Application.UnitTests
UrbanSync.Api.IntegrationTests
```

### Pruebas unitarias

Validan servicios de Application con dependencias simuladas.

### Pruebas de integración

Validan el arranque y comportamiento HTTP de la API.

Comando:

```powershell
dotnet test src/backend/UrbanSync.sln
```

Los nuevos módulos deben incorporar pruebas para sus rutas críticas.

---

## 10. Docker

Docker construye dos imágenes:

```text
urbansync-api
urbansync-web
```

Archivos:

```text
deploy/docker/api.Dockerfile
deploy/docker/web.Dockerfile
docker-compose.yml
```

Los Dockerfiles utilizan compilación multi-stage:

```text
restore
build
runtime
```

Los contenedores:

- Ejecutan en Release.
- Utilizan un usuario no root.
- Escuchan en el puerto interno `8080`.
- Exponen health checks.
- Obtienen secretos mediante variables de entorno.

La Web se comunica con la API mediante el nombre interno del servicio:

```text
http://api:8080/
```

---

## 11. Integración continua

Workflow:

```text
.github/workflows/ci.yml
```

Trabajos:

```text
backend
web
mobile
docker
```

Cada trabajo se ejecuta de manera independiente para detectar con claridad qué componente falla.

El pipeline no debe depender de secretos productivos para compilar o ejecutar pruebas estructurales.

---

## 12. Reglas para nuevos módulos

Un nuevo módulo del backend debe seguir este orden:

```text
Domain
    Entidades o reglas nuevas, cuando correspondan.

Application
    DTO, interfaces y servicios.

Infrastructure
    Repositorios e implementaciones.

Api
    Contratos HTTP y controladores.

Tests
    Pruebas unitarias e integración.
```

Un nuevo módulo Web debe agregar, cuando corresponda:

```text
ApiClients/<Feature>
Presentation/<Feature>
ViewModels
Controller
Views
```

Una nueva funcionalidad Flutter debe ubicarse en:

```text
features/<feature-name>/
├── data/
├── domain/
└── presentation/
```

---

## 13. Decisiones pendientes

Los siguientes temas se completarán después de integrar los módulos funcionales en desarrollo:

- Estrategia definitiva de migraciones.
- Scripts maestros de base de datos.
- Seeds de desarrollo y producción.
- Índices de incidencias y órdenes.
- Auditoría persistente.
- Publicación automática de imágenes.
- Despliegue automático por ambiente.
- Auditoría arquitectónica final.