IF OBJECT_ID('dbo.Activos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Activos
    (
        Id               INT IDENTITY(1,1) PRIMARY KEY,
        Codigo           NVARCHAR(50) NOT NULL UNIQUE,
        Nombre           NVARCHAR(100) NOT NULL,
        Tipo             NVARCHAR(50) NOT NULL,
        Estado           NVARCHAR(30) NOT NULL
            CONSTRAINT DF_Activos_Estado
            DEFAULT ('Operativo'),
        JurisdiccionId   INT NOT NULL,
        FechaInstalacion DATETIME2 NULL,
        Activo           BIT NOT NULL
            CONSTRAINT DF_Activos_Activo
            DEFAULT (1),

        CONSTRAINT FK_Activos_Jurisdicciones
            FOREIGN KEY (JurisdiccionId)
            REFERENCES dbo.Jurisdicciones(Id)
    );
END
GO

/* =========================================================
   Relación entre Incidencias y Activos
   ========================================================= */

IF COL_LENGTH(
    'dbo.Incidencias',
    'ActivoId') IS NULL
BEGIN
    ALTER TABLE dbo.Incidencias
    ADD ActivoId INT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Incidencias_Activos'
)
BEGIN
    ALTER TABLE dbo.Incidencias
    ADD CONSTRAINT FK_Incidencias_Activos
        FOREIGN KEY (ActivoId)
        REFERENCES dbo.Activos(Id);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE
        name = 'IX_Incidencias_ActivoId'
        AND object_id =
            OBJECT_ID('dbo.Incidencias')
)
BEGIN
    CREATE INDEX IX_Incidencias_ActivoId
        ON dbo.Incidencias(ActivoId);
END
GO