# UrbanSync

UrbanSync es una plataforma de gestión de infraestructura urbana orientada al registro, seguimiento, atención y auditoría de incidencias reportadas por la ciudadanía.

El sistema integra una API REST, una aplicación Web MVC y una aplicación móvil Flutter dentro de un monorepo. Su propósito es facilitar la comunicación entre ciudadanos, técnicos, supervisores y administradores, permitiendo documentar el ciclo de vida de los problemas urbanos y las acciones realizadas para resolverlos.

> Proyecto académico desarrollado para la asignatura Desarrollo de Software con Tecnologías Open Source de la Universidad APEC.

---

## Estado del proyecto

UrbanSync se encuentra actualmente en desarrollo activo.

La estructura principal del monorepo, la autenticación JWT, la Web MVC, la aplicación móvil, Docker y el flujo de integración continua ya están configurados.

Algunos módulos funcionales de la API, como incidencias, órdenes de trabajo, reportes y auditoría, continúan en desarrollo y pueden responder temporalmente con `404 Not Found` desde la Web o la aplicación móvil.

---

## Componentes

UrbanSync está compuesto por los siguientes proyectos:

| Componente | Tecnología | Propósito |
|---|---|---|
| API | ASP.NET Core 8 | Autenticación, usuarios, roles y módulos de negocio |
| Web | ASP.NET Core MVC 8 | Interfaz administrativa y operativa |
| Mobile | Flutter | Aplicación para ciudadanos, técnicos y gestores |
| Database | Azure SQL / SQL Server | Persistencia de la información |
| Docker | Docker Compose | Ejecución reproducible de API y Web |
| CI | GitHub Actions | Build, pruebas y validación automática |

---

## Arquitectura general

```text
Flutter Mobile ────────┐
                       │
ASP.NET Core MVC ──────┼──── HTTP / JSON / JWT ──── ASP.NET Core API
                       │                                  │
Otros clientes ────────┘                                  │
                                                          ▼
                                                    Azure SQL
```

La API utiliza una arquitectura dividida en cuatro proyectos:

```text
UrbanSync.Api
    │
    ├── UrbanSync.Application
    │       │
    │       └── UrbanSync.Domain
    │
    └── UrbanSync.Infrastructure
            │
            ├── UrbanSync.Application
            └── UrbanSync.Domain
```

Las responsabilidades principales son:

- `UrbanSync.Domain`: entidades y conceptos centrales del negocio.
- `UrbanSync.Application`: casos de uso, DTO, servicios e interfaces.
- `UrbanSync.Infrastructure`: persistencia, seguridad y servicios externos.
- `UrbanSync.Api`: contratos HTTP, controladores, autenticación y middleware.

Consulta la documentación completa en:

```text
docs/architecture/README.md
```

---

## Estructura del monorepo

```text
UrbanSync/
├── .github/
│   └── workflows/
│       └── ci.yml
├── database/
│   ├── migrations/
│   ├── scripts/
│   └── seeds/
├── deploy/
│   └── docker/
│       ├── api.Dockerfile
│       └── web.Dockerfile
├── docs/
│   ├── architecture/
│   ├── api/
│   ├── database/
│   ├── mobile/
│   ├── project-management/
│   └── web/
├── scripts/
├── src/
│   ├── backend/
│   │   ├── UrbanSync.Api/
│   │   ├── UrbanSync.Application/
│   │   ├── UrbanSync.Domain/
│   │   ├── UrbanSync.Infrastructure/
│   │   └── UrbanSync.sln
│   ├── mobile/
│   └── web/
│       └── UrbanSync.Web/
├── tests/
│   └── backend/
│       ├── UrbanSync.Api.IntegrationTests/
│       └── UrbanSync.Application.UnitTests/
├── .dockerignore
├── .env.example
├── .gitignore
├── Directory.Build.props
├── Directory.Packages.props
├── docker-compose.yml
├── global.json
└── README.md
```

---

## Requisitos

Para trabajar con todo el proyecto se recomienda instalar:

- .NET SDK `8.0.421` o una revisión compatible de .NET 8.
- Flutter estable compatible con Dart `3.12.2` o superior dentro de la misma versión mayor requerida.
- Android Studio o un dispositivo Android para ejecutar la aplicación móvil.
- Docker Desktop con contenedores Linux.
- Git.
- Acceso a SQL Server o Azure SQL.

Puedes comprobar las herramientas principales con:

```powershell
dotnet --version
flutter --version
docker version
git --version
```

---

## Configuración local

La API requiere una cadena de conexión y configuración JWT.

El archivo de configuración local de desarrollo no debe contener secretos versionados. Para configurar la cadena mediante User Secrets:

```powershell
dotnet user-secrets init `
  --project src/backend/UrbanSync.Api/UrbanSync.Api.csproj
```

```powershell
dotnet user-secrets set `
  "ConnectionStrings:UrbanSyncDb" `
  "TU_CADENA_DE_CONEXION" `
  --project src/backend/UrbanSync.Api/UrbanSync.Api.csproj
```

La configuración JWT de desarrollo se encuentra en:

```text
src/backend/UrbanSync.Api/appsettings.Development.json
```

Nunca deben subirse claves, contraseñas o cadenas de conexión reales al repositorio.

Consulta la guía completa en:

```text
docs/local-development.md
```

---

## Ejecutar el backend

Desde la raíz:

```powershell
dotnet restore src/backend/UrbanSync.sln
```

```powershell
dotnet build src/backend/UrbanSync.sln
```

```powershell
dotnet test src/backend/UrbanSync.sln
```

Para iniciar la API:

```powershell
dotnet run `
  --project src/backend/UrbanSync.Api/UrbanSync.Api.csproj
```

Direcciones locales:

```text
API HTTP:  http://localhost:5119
Swagger:   http://localhost:5119/swagger
Health:    http://localhost:5119/health
```

Swagger se habilita únicamente en el ambiente `Development`.

---

## Ejecutar la Web MVC

Mantén la API ejecutándose y abre otra terminal:

```powershell
dotnet run `
  --project src/web/UrbanSync.Web/UrbanSync.Web.csproj
```

Dirección local:

```text
http://localhost:5019
```

Health check:

```text
http://localhost:5019/health
```

La URL de la API utilizada por la Web se configura mediante:

```text
UrbanSyncApi:BaseUrl
```

---

## Ejecutar Flutter

Desde la carpeta móvil:

```powershell
cd src/mobile
```

```powershell
flutter pub get
```

```powershell
flutter analyze
```

```powershell
flutter test
```

Lista los dispositivos:

```powershell
flutter devices
```

Lista los emuladores disponibles:

```powershell
flutter emulators
```

Ejemplo para iniciar el emulador configurado:

```powershell
flutter emulators --launch Medium_Phone_API_36.0
```

Ejecuta la aplicación:

```powershell
flutter run -d emulator-5554
```

Desde un emulador Android, la aplicación accede a la API local mediante:

```text
http://10.0.2.2:5119/
```

`10.0.2.2` representa el equipo anfitrión desde el emulador Android.

---

## Ejecutar con Docker

Copia el archivo de ejemplo:

```powershell
Copy-Item .env.example .env
```

Completa los valores de `.env` y valida la configuración:

```powershell
docker compose config
```

Construye las imágenes:

```powershell
docker compose build
```

Inicia los servicios:

```powershell
docker compose up -d
```

Comprueba su estado:

```powershell
docker compose ps
```

Direcciones:

```text
API:  http://localhost:8080
Web:  http://localhost:8081
```

Health checks:

```text
http://localhost:8080/health
http://localhost:8081/health
```

Para detener los servicios:

```powershell
docker compose down
```

El archivo `.env` contiene secretos locales y está excluido de Git.

---

## Integración continua

El workflow se encuentra en:

```text
.github/workflows/ci.yml
```

GitHub Actions ejecuta cuatro trabajos:

- Build y pruebas del backend.
- Build y publicación de la Web.
- Análisis, pruebas y APK debug de Flutter.
- Validación y construcción de las imágenes Docker.

El workflow se ejecuta mediante:

- Push a `main`.
- Push a `develop`.
- Pull request hacia `main`.
- Pull request hacia `develop`.
- Ejecución manual cuando esté disponible desde la rama predeterminada.

---

## Convenciones básicas

### Ramas

Ejemplos:

```text
feature/add-incidents-module
fix/mobile-session-expiration
refactor/standardize-project-structure
build/add-ci-pipeline
docs/update-local-setup
```

### Commits

El proyecto utiliza Conventional Commits:

```text
feat: add incident registration
fix: handle expired mobile session
refactor: extract presentation services
test: add authentication tests
build: containerize api and web applications
docs: document local development
```

### Antes de subir cambios

Ejecuta como mínimo:

```powershell
dotnet build src/backend/UrbanSync.sln
dotnet test src/backend/UrbanSync.sln
dotnet build src/web/UrbanSync.Web/UrbanSync.Web.csproj
```

Para cambios móviles:

```powershell
cd src/mobile
flutter analyze
flutter test
```

Para cambios de infraestructura:

```powershell
docker compose config
docker compose build
```

---

## Seguridad

No deben agregarse al repositorio:

- Contraseñas.
- Cadenas de conexión reales.
- Claves JWT.
- Archivos `.env`.
- `appsettings.Development.json` con secretos.
- Certificados privados.
- Tokens personales.
- Archivos generados por compilación.

Cuando una credencial se expone accidentalmente, debe rotarse inmediatamente aunque luego se elimine del repositorio.

---

## Equipo de desarrollo

Proyecto desarrollado por estudiantes de la Universidad APEC:

- David Rivas — A00117072
- Romer Amparo — A00118532
- Elwins Zorrilla — A00118365
- Carlos Rodríguez — A00116172
- Renny Placencio — A00119098

Facilitador:

- Omar Reyes

---

## Documentación

```text
docs/architecture/README.md   Arquitectura del sistema
docs/local-development.md    Configuración y ejecución local
docs/mobile/                  Documentación de Flutter
docs/web/                     Documentación y capturas Web
docs/project-management/      Planificación y Scrum
```

La documentación de base de datos se completará cuando finalicen los módulos funcionales actualmente en desarrollo.

---

## Licencia

Este repositorio fue creado con fines académicos. La licencia definitiva deberá ser acordada por los propietarios del proyecto antes de cualquier distribución externa.