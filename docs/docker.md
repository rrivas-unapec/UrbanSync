# Docker en UrbanSync

## 1. Propósito

UrbanSync utiliza Docker para ejecutar de forma reproducible:

- La API ASP.NET Core.
- La Web ASP.NET Core MVC.

Actualmente, la base de datos no se levanta mediante Docker Compose. La API utiliza una cadena de conexión proporcionada mediante variables de entorno.

La incorporación de SQL Server local y scripts automáticos queda pendiente hasta que finalicen los módulos funcionales y el esquema definitivo de base de datos.

---

## 2. Archivos relacionados

```text
docker-compose.yml
.dockerignore
.env.example

deploy/
└── docker/
    ├── api.Dockerfile
    └── web.Dockerfile