# Guía de contribución de UrbanSync

## 1. Propósito

Esta guía define las reglas básicas para contribuir a UrbanSync de manera ordenada, segura y fácil de revisar.

Cada integrante debe trabajar desde una rama independiente. Los cambios deben integrarse mediante pull requests y pasar las validaciones automáticas antes del merge.

---

## 2. Ramas principales

El repositorio puede utilizar las siguientes ramas principales:

```text
main
develop
```

### `main`

Representa la versión estable o entregable del proyecto.

No deben realizarse commits directos sobre `main`, salvo autorización expresa del propietario del repositorio.

### `develop`

Puede utilizarse como rama de integración antes de llevar los cambios a `main`.

La rama de destino de cada pull request debe acordarse con el propietario o responsable del repositorio.

---

## 3. Crear una rama de trabajo

Primero, actualiza las referencias remotas:

```powershell
git fetch origin
```

Si el equipo utiliza `develop` como rama de integración:

```powershell
git checkout develop
git pull origin develop
```

Después, crea tu rama:

```powershell
git checkout -b feature/add-incidents-module
```

Si el equipo trabaja directamente desde `main`:

```powershell
git checkout main
git pull origin main
git checkout -b feature/add-incidents-module
```

---

## 4. Convención para nombres de ramas

Los nombres deben ser breves, descriptivos y escritos en minúsculas.

### Nuevas funcionalidades

```text
feature/add-incidents-module
feature/create-work-orders
feature/add-audit-endpoint
```

### Correcciones

```text
fix/mobile-session-expiration
fix/web-api-base-url
fix/docker-health-check
```

### Refactorizaciones

```text
refactor/standardize-project-structure
refactor/extract-web-presentation-services
```

### Infraestructura

```text
build/add-ci-pipeline
build/containerize-applications
```

### Documentación

```text
docs/update-local-setup
docs/document-architecture
```

### Pruebas

```text
test/add-authentication-tests
test/add-incidents-integration-tests
```

---

## 5. Convención para commits

UrbanSync utiliza Conventional Commits.

El formato general es:

```text
tipo: descripción breve
```

Tipos recomendados:

| Tipo | Uso |
|---|---|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de un error |
| `refactor` | Cambio interno sin alterar el comportamiento |
| `test` | Creación o modificación de pruebas |
| `build` | Docker, dependencias, compilación o CI/CD |
| `docs` | Documentación |
| `chore` | Mantenimiento general |
| `perf` | Mejora de rendimiento |
| `style` | Formato sin cambio funcional |

Ejemplos correctos:

```text
feat: add citizen registration endpoint
fix: handle expired jwt session
refactor: split web api clients by feature
test: add usuario service tests
build: add docker and continuous integration
docs: document local development
```

Evita mensajes ambiguos como:

```text
changes
fix stuff
update
final
commit 2
```

---

## 6. Alcance de los commits

Cada commit debe representar una unidad de trabajo coherente.

No mezcles en un mismo commit:

- Refactorizaciones no relacionadas.
- Cambios visuales y cambios de infraestructura.
- Nuevas funcionalidades y documentación ajena.
- Correcciones diferentes.
- Archivos generados.

Ejemplo incorrecto:

```text
feat: add incidents and change docker and update readme
```

Ejemplo recomendado:

```text
feat: add incidents module
build: update api docker image
docs: document incidents endpoints
```

---

## 7. Revisar cambios antes del commit

Comprueba el estado del repositorio:

```powershell
git status
```

Revisa los cambios no preparados:

```powershell
git diff
```

Después de ejecutar `git add`, revisa los cambios preparados:

```powershell
git diff --staged
```

No deben incluirse:

```text
.env
bin/
obj/
build/
.dart_tool/
.vs/
contraseñas
claves privadas
tokens
cadenas de conexión reales
```

---

## 8. Validaciones del backend

Restaura las dependencias:

```powershell
dotnet restore src/backend/UrbanSync.sln
```

Compila:

```powershell
dotnet build src/backend/UrbanSync.sln
```

Ejecuta las pruebas:

```powershell
dotnet test src/backend/UrbanSync.sln
```

Para una validación equivalente al pipeline:

```powershell
dotnet build src/backend/UrbanSync.sln --configuration Release
```

```powershell
dotnet test src/backend/UrbanSync.sln --configuration Release
```

---

## 9. Validaciones de la Web

Restaura:

```powershell
dotnet restore src/web/UrbanSync.Web/UrbanSync.Web.csproj
```

Compila:

```powershell
dotnet build src/web/UrbanSync.Web/UrbanSync.Web.csproj
```

Para validar en Release:

```powershell
dotnet build src/web/UrbanSync.Web/UrbanSync.Web.csproj --configuration Release
```

---

## 10. Validaciones de Flutter

Entra al proyecto móvil:

```powershell
cd src/mobile
```

Instala las dependencias:

```powershell
flutter pub get
```

Ejecuta el análisis estático:

```powershell
flutter analyze
```

Ejecuta las pruebas:

```powershell
flutter test
```

Cuando el cambio afecte la compilación de Android:

```powershell
flutter build apk --debug
```

Regresa a la raíz:

```powershell
cd ../..
```

---

## 11. Validaciones de Docker

Valida Docker Compose:

```powershell
docker compose config
```

Construye las imágenes:

```powershell
docker compose build
```

Cuando el cambio afecte la ejecución:

```powershell
docker compose up -d
```

Comprueba el estado:

```powershell
docker compose ps
```

Al finalizar:

```powershell
docker compose down
```

---

## 12. Preparar el commit

Agrega los archivos:

```powershell
git add .
```

Comprueba nuevamente:

```powershell
git status
```

Realiza el commit:

```powershell
git commit -m "tipo: descripción"
```

Ejemplo:

```powershell
git commit -m "docs: document contribution workflow"
```

---

## 13. Subir la rama

Para el primer push:

```powershell
git push -u origin nombre-de-la-rama
```

Para los pushes siguientes:

```powershell
git push
```

No utilices `git push --force` en una rama compartida.

Cuando sea estrictamente necesario reescribir una rama propia, utiliza:

```powershell
git push --force-with-lease
```

Esto debe acordarse previamente si otras personas trabajan sobre la misma rama.

---

## 14. Crear un pull request

El pull request debe incluir un título claro.

Ejemplo:

```text
refactor: standardize project structure
```

La descripción recomendada es:

```markdown
## Resumen

Descripción breve del objetivo del cambio.

## Cambios realizados

- Cambio uno.
- Cambio dos.
- Cambio tres.

## Cómo probar

1. Ejecutar el comando correspondiente.
2. Abrir la pantalla o endpoint.
3. Verificar el resultado esperado.

## Evidencias

Capturas, pruebas o logs relevantes.

## Pendientes

Aspectos que quedan fuera del alcance del pull request.
```

---

## 15. Revisión antes del merge

Antes de completar un pull request:

- GitHub Actions debe estar en verde.
- No debe haber conflictos con la rama de destino.
- No deben existir secretos en los cambios.
- El backend debe compilar.
- La Web debe compilar.
- Las pruebas deben pasar.
- Flutter debe analizarse y probarse cuando corresponda.
- Docker debe construirse cuando el cambio lo afecte.
- La documentación debe actualizarse cuando sea necesario.
- Deben integrarse los cambios pendientes del resto del equipo.

Un pull request estructural no debe completarse mientras falten cambios funcionales importantes que deban incorporarse primero, salvo acuerdo del equipo.

---

## 16. Integrar cambios de otro compañero

Antes de traer cambios, comprueba que el árbol esté limpio:

```powershell
git status
```

Actualiza las referencias:

```powershell
git fetch origin
```

Para integrar una rama mediante merge:

```powershell
git merge origin/nombre-de-la-rama
```

También puede utilizarse rebase cuando el equipo lo acuerde:

```powershell
git rebase origin/nombre-de-la-rama
```

No mezcles merge y rebase sin entender cómo afectan el historial.

Después de integrar los cambios, ejecuta:

```powershell
dotnet build src/backend/UrbanSync.sln
dotnet test src/backend/UrbanSync.sln
dotnet build src/web/UrbanSync.Web/UrbanSync.Web.csproj
```

También valida Flutter y Docker cuando los cambios los afecten.

---

## 17. Resolver conflictos

Comprueba los conflictos:

```powershell
git status
```

Abre cada archivo marcado y decide manualmente qué código debe conservarse.

Después de resolver un archivo:

```powershell
git add ruta-del-archivo
```

Si realizaste un merge:

```powershell
git commit
```

Si estabas realizando un rebase:

```powershell
git rebase --continue
```

No elimines automáticamente el trabajo de otro integrante sin revisar el propósito de sus cambios.

---

## 18. Seguridad

Nunca deben subirse al repositorio:

- Contraseñas.
- Cadenas de conexión reales.
- Claves JWT.
- Tokens personales.
- Credenciales de Azure.
- Certificados privados.
- Archivos `.env`.
- Secretos de servicios externos.

Cuando una credencial se expone:

1. Elimínala del archivo.
2. Cámbiala inmediatamente en el proveedor.
3. Actualiza el entorno local.
4. Revisa el historial del repositorio.
5. Informa al responsable del proyecto.
6. No asumas que borrar el archivo vuelve segura la credencial.

Una credencial expuesta debe considerarse comprometida.

---

## 19. Definición de terminado

Un cambio se considera terminado cuando:

- Cumple el objetivo acordado.
- Compila correctamente.
- Las pruebas relacionadas pasan.
- No contiene secretos.
- Respeta la estructura del proyecto.
- Utiliza nombres coherentes.
- Está documentado cuando corresponde.
- GitHub Actions finaliza correctamente.
- Puede ser comprendido y mantenido por otro integrante.