# Integración Continua (CI/CD)

## 1. Propósito

UrbanSync utiliza GitHub Actions para validar automáticamente la calidad del código antes de integrar cambios mediante Pull Requests.

Actualmente el pipeline ejecuta compilaciones y validaciones automáticas para los proyectos Backend, Web, Mobile y Docker.

El objetivo es detectar errores antes de que lleguen a la rama principal del repositorio.

---

## 2. Flujo general

Cada vez que un desarrollador crea o actualiza un Pull Request, GitHub ejecuta automáticamente el pipeline de validación.

```text
Developer
      │
      ▼
Push a una rama
      │
      ▼
Pull Request
      │
      ▼
GitHub Actions
      │
      ├───────────────► Backend
      │
      ├───────────────► Web
      │
      ├───────────────► Flutter
      │
      └───────────────► Docker
      │
      ▼
Todos los checks en verde
      │
      ▼
Merge
```

---

## 3. Pipeline actual

Actualmente el repositorio ejecuta cuatro validaciones independientes.

### Backend

Valida el proyecto ASP.NET Core.

Incluye:

- Restauración de paquetes.
- Compilación.
- Ejecución de pruebas unitarias.

---

### Web

Valida el proyecto ASP.NET MVC.

Incluye:

- Restauración.
- Compilación.

---

### Flutter

Valida la aplicación móvil.

Incluye:

- flutter pub get
- flutter analyze
- flutter test

---

### Docker

Comprueba que las imágenes puedan construirse correctamente.

Incluye:

- docker compose config
- docker compose build

---

## 4. Estructura del workflow

Los workflows se encuentran en:

```text
.github/
└── workflows/
    └── ci.yml
```

En el futuro podrán añadirse otros workflows independientes.

Ejemplo:

```text
.github/
└── workflows/
    ├── ci.yml
    ├── release.yml
    ├── deploy-api.yml
    ├── deploy-web.yml
    └── security.yml
```

---

## 5. ¿Cuándo se ejecuta?

Actualmente el pipeline se ejecuta cuando:

- Se crea un Pull Request.
- Se actualiza un Pull Request existente.

En el futuro podrá ejecutarse también sobre pushes directos a ramas protegidas.

---

## 6. Estados posibles

GitHub Actions puede mostrar los siguientes estados.

### En ejecución

```text
🟡 In Progress
```

Significa que las validaciones todavía están ejecutándose.

---

### Correcto

```text
🟢 Success
```

Todas las validaciones finalizaron correctamente.

El Pull Request puede revisarse.

---

### Error

```text
🔴 Failed
```

Al menos una validación falló.

Debe corregirse antes del merge.

---

## 7. Validaciones recomendadas antes de hacer Push

Aunque GitHub ejecuta las validaciones automáticamente, se recomienda ejecutarlas primero de manera local.

### Backend

```powershell
dotnet restore src/backend/UrbanSync.sln

dotnet build src/backend/UrbanSync.sln

dotnet test src/backend/UrbanSync.sln
```

---

### Web

```powershell
dotnet build src/web/UrbanSync.Web/UrbanSync.Web.csproj
```

---

### Mobile

```powershell
cd src/mobile

flutter pub get

flutter analyze

flutter test

cd ../..
```

---

### Docker

```powershell
docker compose config

docker compose build
```

---

## 8. Buenas prácticas

Antes de abrir un Pull Request:

- Verificar que el proyecto compile.
- Ejecutar las pruebas.
- Confirmar que Docker construye correctamente.
- Revisar los cambios con `git diff`.
- No incluir archivos temporales.
- No subir secretos.
- Escribir commits descriptivos.

---

## 9. Futuras mejoras

Cuando UrbanSync entre en una etapa más avanzada, el pipeline podrá ampliarse con:

- Publicación automática en Azure.
- Publicación automática en Render.
- Despliegue de la aplicación Web.
- Publicación automática de APK.
- Análisis de cobertura.
- Escaneo de vulnerabilidades.
- Análisis estático de código.
- Versionado automático.
- Generación automática de artefactos.
- Notificaciones mediante correo o Teams.

---

## 10. Estado actual

Actualmente el pipeline valida correctamente:

- Backend (.NET)
- Web (.NET MVC)
- Mobile (Flutter)
- Docker

La fase de despliegue continuo (CD) todavía no forma parte del proyecto y será implementada cuando la aplicación alcance una versión estable.