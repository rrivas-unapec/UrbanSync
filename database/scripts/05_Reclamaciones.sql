/* ============================================================
   Proyecto: UrbanSync
   Tabla: Reclamaciones (reclamo directo del ciudadano, ligado
   a una ubicacion — respaldo de ClaimController/ClaimRepository)
   Ejecutar en el contexto de: UrbanSync
   Requiere haber ejecutado antes: 01_Epica1_UsuariosRolesPermisos.sql,
   02_Epica2_InteligenciaNegocio.sql
   ============================================================ */

USE UrbanSync;
GO

IF OBJECT_ID('dbo.Reclamaciones', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reclamaciones
    (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioCiudadanoId  INT NOT NULL,
        UbicacionId         INT NOT NULL,
        Categoria           NVARCHAR(50) NOT NULL,
        Titulo              NVARCHAR(150) NOT NULL,
        Descripcion         NVARCHAR(1000) NOT NULL,
        Estado              NVARCHAR(30) NOT NULL
            CONSTRAINT DF_Reclamaciones_Estado
            DEFAULT ('Pendiente'), -- Pendiente, EnRevision, Resuelta, Rechazada
        FechaCreacion       DATETIME2 NOT NULL
            CONSTRAINT DF_Reclamaciones_FechaCreacion
            DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_Reclamaciones_Usuarios FOREIGN KEY (UsuarioCiudadanoId)
            REFERENCES dbo.Usuarios(Id),
        CONSTRAINT FK_Reclamaciones_Ubicaciones FOREIGN KEY (UbicacionId)
            REFERENCES dbo.Ubicaciones(Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reclamaciones_UsuarioCiudadanoId')
    CREATE INDEX IX_Reclamaciones_UsuarioCiudadanoId ON dbo.Reclamaciones(UsuarioCiudadanoId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reclamaciones_Estado')
    CREATE INDEX IX_Reclamaciones_Estado ON dbo.Reclamaciones(Estado);
GO

PRINT 'Tabla Reclamaciones creada correctamente.';
GO
