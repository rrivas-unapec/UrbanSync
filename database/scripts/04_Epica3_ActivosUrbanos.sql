USE UrbanSync;
GO

IF OBJECT_ID('dbo.Activos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Activos
    (
        Id               INT IDENTITY(1,1) PRIMARY KEY,
        Codigo           NVARCHAR(50) NOT NULL UNIQUE,
        Nombre           NVARCHAR(100) NOT NULL,
        Tipo             NVARCHAR(50) NOT NULL, -- Transformador, Poste, Semaforo, Tapa Alcantarilla
        Estado           NVARCHAR(30) NOT NULL DEFAULT ('Operativo'),
        JurisdiccionId   INT NOT NULL,
        FechaInstalacion DATETIME2 NULL,
        Activo           BIT NOT NULL DEFAULT (1),
        CONSTRAINT FK_Activos_Jurisdicciones FOREIGN KEY (JurisdiccionId)
            REFERENCES dbo.Jurisdicciones(Id)
    );
END
GO