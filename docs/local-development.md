# Desarrollo local de UrbanSync

## 1. Propósito

Esta guía explica cómo preparar y ejecutar UrbanSync en un ambiente local.

Los componentes principales son:

```text
API ASP.NET Core
Web ASP.NET Core MVC
Flutter Mobile
Azure SQL o SQL Server
Docker
```

---

## 2. Requisitos

Instala:

- Git.
- .NET SDK 8.
- Flutter estable.
- Android Studio.
- Docker Desktop.
- Un editor como Visual Studio Code, Visual Studio o Rider.
- Acceso a Azure SQL o SQL Server.

Versiones definidas por el repositorio:

```text
.NET SDK: 8.0.421
Target Framework: net8.0
Dart SDK: ^3.12.2
```

Verifica:

```powershell
git --version
dotnet --version
flutter --version
docker version
```

Diagnóstico de Flutter:

```powershell
flutter doctor
```

---

## 3. Clonar el repositorio

```powershell
git clone URL_DEL_REPOSITORIO
```

```powershell
cd UrbanSync
```

Comprueba la rama:

```powershell
git branch --show-current
```

---

## 4. Secretos locales de la API

La API espera:

```text
ConnectionStrings:UrbanSyncDb
Jwt:Issuer
Jwt:Audience
Jwt:SecretKey
Jwt:ExpirationMinutes
```

La cadena de conexión no debe colocarse en archivos versionados.

Inicializa User Secrets:

```powershell
dotnet user-secrets init `
  --project src/backend/UrbanSync.Api/UrbanSync.Api.csproj
```

Configura la conexión:

```powershell
dotnet user-secrets set `
  "ConnectionStrings:UrbanSyncDb" `
  "TU_CADENA_DE_CONEXION" `
  --project src/backend/UrbanSync.Api/UrbanSync.Api.csproj
```

Lista los secretos:

```powershell
dotnet user-secrets list `
  --project src/backend/UrbanSync.Api/UrbanSync.Api.csproj
```

Elimina un secreto:

```powershell
dotnet user-secrets remove `
  "ConnectionStrings:UrbanSyncDb" `
  --project src/backend/UrbanSync.Api/UrbanSync.Api.csproj
```

Nunca compartas la salida de User Secrets en capturas, logs o conversaciones.

---

## 5. Backend

### Restaurar

```powershell
dotnet restore src/backend/UrbanSync.sln
```

### Compilar

```powershell
dotnet build src/backend/UrbanSync.sln
```

### Probar

```powershell
dotnet test src/backend/UrbanSync.sln
```

### Ejecutar API

```powershell
dotnet run `
  --project src/backend/UrbanSync.Api/UrbanSync.Api.csproj
```

La API utiliza normalmente:

```text
http://localhost:5119
```

Swagger:

```text
http://localhost:5119/swagger
```

Health:

```text
http://localhost:5119/health
```

Para recarga automática:

```powershell
dotnet watch `
  --project src/backend/UrbanSync.Api/UrbanSync.Api.csproj `
  run
```

---

## 6. Web MVC

La Web necesita que la API esté disponible.

Ejecuta en otra terminal:

```powershell
dotnet run `
  --project src/web/UrbanSync.Web/UrbanSync.Web.csproj
```

Dirección:

```text
http://localhost:5019
```

Health:

```text
http://localhost:5019/health
```

Para recarga automática:

```powershell
dotnet watch `
  --project src/web/UrbanSync.Web/UrbanSync.Web.csproj `
  run
```

La configuración local predeterminada apunta a:

```text
http://localhost:5119/
```

mediante:

```text
UrbanSyncApi:BaseUrl
```

---

## 7. Flutter

Entra en:

```powershell
cd src/mobile
```

Instala dependencias:

```powershell
flutter pub get
```

Analiza:

```powershell
flutter analyze
```

Ejecuta pruebas:

```powershell
flutter test
```

Lista dispositivos:

```powershell
flutter devices
```

Lista emuladores:

```powershell
flutter emulators
```

Inicia el emulador configurado:

```powershell
flutter emulators --launch Medium_Phone_API_36.0
```

Vuelve a comprobar:

```powershell
flutter devices
```

Ejecuta:

```powershell
flutter run -d emulator-5554
```

El identificador puede variar. Utiliza el mostrado por `flutter devices`.

### Dirección de la API en Android Emulator

No uses:

```text
http://localhost:5119/
```

dentro del emulador.

Usa:

```text
http://10.0.2.2:5119/
```

Desde Android Emulator, `10.0.2.2` apunta al equipo anfitrión.

### Sobrescribir la URL

La aplicación admite:

```text
API_BASE_URL
```

Ejemplo:

```powershell
flutter run `
  --dart-define=API_BASE_URL=http://10.0.2.2:5119/
```

---

## 8. Docker

Desde la raíz:

```powershell
Copy-Item .env.example .env
```

Edita `.env` con valores locales.

Valida:

```powershell
docker compose config
```

Construye:

```powershell
docker compose build
```

Inicia:

```powershell
docker compose up -d
```

Comprueba:

```powershell
docker compose ps
```

Direcciones:

```text
API:  http://localhost:8080
Web:  http://localhost:8081
```

Logs:

```powershell
docker compose logs -f
```

Solo API:

```powershell
docker compose logs -f api
```

Solo Web:

```powershell
docker compose logs -f web
```

Detener:

```powershell
docker compose down
```

Reconstruir:

```powershell
docker compose up -d --build
```

---

## 9. Variables de `.env`

Ejemplo:

```dotenv
URBANSYNC_DB_CONNECTION=TU_CADENA_DE_CONEXION

JWT_ISSUER=UrbanSync.Api
JWT_AUDIENCE=UrbanSync.Clients
JWT_SECRET_KEY=UNA_CLAVE_DE_AL_MENOS_32_CARACTERES
JWT_EXPIRATION_MINUTES=60
```

El archivo `.env` está ignorado por Git.

Comprueba:

```powershell
git check-ignore .env
```

Debe devolver:

```text
.env
```

---

## 10. Flujo diario recomendado

Actualiza tu rama:

```powershell
git fetch origin
```

```powershell
git pull
```

Ejecuta build y pruebas antes de comenzar:

```powershell
dotnet build src/backend/UrbanSync.sln
dotnet test src/backend/UrbanSync.sln
```

Después de trabajar:

```powershell
git status
```

```powershell
git diff
```

Ejecuta las validaciones relacionadas con tus cambios.

Realiza el commit:

```powershell
git add .
git commit -m "tipo: descripcion"
```

Sube:

```powershell
git push
```

---

## 11. Solución de problemas

### Docker no puede conectarse

Error típico:

```text
The system cannot find the file specified.
dockerDesktopLinuxEngine
```

Solución:

- Abre Docker Desktop.
- Espera a que el motor esté ejecutándose.
- Comprueba `docker version`.
- Deben aparecer las secciones Client y Server.

### Contenedor unhealthy

Comprueba:

```powershell
docker compose ps
docker logs urbansync-api
docker logs urbansync-web
```

### Flutter no encuentra dispositivo

Comprueba:

```powershell
flutter devices
flutter emulators
```

Inicia un emulador y vuelve a ejecutar.

### Aplicación móvil devuelve 404

Un `404 Recurso no encontrado` puede indicar que el endpoint funcional todavía no ha sido implementado en la API. No necesariamente representa un error del cliente móvil.

### Web no conecta con la API

Comprueba:

- Que la API esté ejecutándose.
- Que la URL sea correcta.
- Que `UrbanSyncApi:BaseUrl` tenga barra final.
- Que dentro de Docker se utilice `http://api:8080/`.

### Restore de NuGet inconsistente

Elimina `bin` y `obj` de los proyectos afectados y ejecuta:

```powershell
dotnet restore src/backend/UrbanSync.sln --force --no-cache
```

No subas `bin` ni `obj` al repositorio.

---

## 12. Validación completa

Backend:

```powershell
dotnet restore src/backend/UrbanSync.sln
dotnet build src/backend/UrbanSync.sln --configuration Release
dotnet test src/backend/UrbanSync.sln --configuration Release
```

Web:

```powershell
dotnet restore src/web/UrbanSync.Web/UrbanSync.Web.csproj
dotnet build src/web/UrbanSync.Web/UrbanSync.Web.csproj --configuration Release
```

Flutter:

```powershell
cd src/mobile
flutter pub get
flutter analyze
flutter test
```

Docker:

```powershell
cd ../..
docker compose config
docker compose build
docker compose up -d
docker compose ps
```

Al finalizar:

```powershell
docker compose down
```