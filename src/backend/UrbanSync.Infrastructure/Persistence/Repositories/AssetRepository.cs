using System.Data;
using Microsoft.Data.SqlClient;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Asset;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories;

public sealed class AssetRepository : IAssetRepository
{
    private readonly IDbConnectionFactory
        _connectionFactory;

    public AssetRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory =
            connectionFactory;
    }

    public async Task<IReadOnlyList<AssetDto>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        var assets =
            new List<AssetDto>();

        using var connection =
            _connectionFactory.CreateConnection();

        using var command =
            new SqlCommand(
                """
                SELECT
                    a.Id,
                    a.Codigo,
                    a.Nombre,
                    a.Tipo,
                    a.Estado,
                    a.JurisdiccionId,
                    j.Nombre AS NombreJurisdiccion,
                    a.FechaInstalacion,
                    a.Activo
                FROM dbo.Activos a
                INNER JOIN dbo.Jurisdicciones j
                    ON a.JurisdiccionId = j.Id
                WHERE a.Activo = 1
                ORDER BY a.Id DESC;
                """,
                connection);

        await connection.OpenAsync(
            cancellationToken);

        using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        while (await reader.ReadAsync(
                   cancellationToken))
        {
            assets.Add(
                MapReaderToDto(reader));
        }

        return assets;
    }

    public async Task<AssetDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command =
            new SqlCommand(
                """
                SELECT
                    a.Id,
                    a.Codigo,
                    a.Nombre,
                    a.Tipo,
                    a.Estado,
                    a.JurisdiccionId,
                    j.Nombre AS NombreJurisdiccion,
                    a.FechaInstalacion,
                    a.Activo
                FROM dbo.Activos a
                INNER JOIN dbo.Jurisdicciones j
                    ON a.JurisdiccionId = j.Id
                WHERE a.Id = @Id;
                """,
                connection);

        command.Parameters.Add(
            "@Id",
            SqlDbType.Int).Value = id;

        await connection.OpenAsync(
            cancellationToken);

        using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        return await reader.ReadAsync(
            cancellationToken)
            ? MapReaderToDto(reader)
            : null;
    }

    public async Task<IReadOnlyList<AssetHistoryDto>>
        GetHistoryByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
    {
        var history =
            new List<AssetHistoryDto>();

        using var connection =
            _connectionFactory.CreateConnection();

        using var command =
            new SqlCommand(
                """
                SELECT
                    i.Id AS IncidentId,
                    i.CodigoCaso,
                    ti.Nombre AS IncidentType,
                    i.Descripcion,
                    i.Estado,
                    i.FechaReporte
                FROM dbo.Incidencias i
                INNER JOIN dbo.TiposIncidencia ti
                    ON i.TipoIncidenciaId = ti.Id
                WHERE i.ActivoId = @AssetId
                ORDER BY i.FechaReporte DESC;
                """,
                connection);

        command.Parameters.Add(
            "@AssetId",
            SqlDbType.Int).Value = id;

        await connection.OpenAsync(
            cancellationToken);

        using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        while (await reader.ReadAsync(
                   cancellationToken))
        {
            history.Add(
                new AssetHistoryDto
                {
                    IncidentId =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "IncidentId")),

                    CaseCode =
                        reader.GetString(
                            reader.GetOrdinal(
                                "CodigoCaso")),

                    IncidentType =
                        reader.GetString(
                            reader.GetOrdinal(
                                "IncidentType")),

                    Description =
                        reader.GetString(
                            reader.GetOrdinal(
                                "Descripcion")),

                    Status =
                        reader.GetString(
                            reader.GetOrdinal(
                                "Estado")),

                    ReportDate =
                        reader.GetDateTime(
                            reader.GetOrdinal(
                                "FechaReporte"))
                });
        }

        return history;
    }

    public async Task<int> CreateAsync(
        CreateAssetDto dto,
        CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command =
            new SqlCommand(
                """
                INSERT INTO dbo.Activos
                (
                    Codigo,
                    Nombre,
                    Tipo,
                    Estado,
                    JurisdiccionId,
                    FechaInstalacion
                )
                OUTPUT INSERTED.Id
                VALUES
                (
                    @Codigo,
                    @Nombre,
                    @Tipo,
                    @Estado,
                    @JurisdiccionId,
                    ISNULL(
                        @FechaInstalacion,
                        SYSDATETIME()
                    )
                );
                """,
                connection);

        command.Parameters.Add(
            "@Codigo",
            SqlDbType.NVarChar,
            50).Value = dto.Code;

        command.Parameters.Add(
            "@Nombre",
            SqlDbType.NVarChar,
            100).Value = dto.Name;

        command.Parameters.Add(
            "@Tipo",
            SqlDbType.NVarChar,
            50).Value = dto.Type;

        command.Parameters.Add(
            "@Estado",
            SqlDbType.NVarChar,
            30).Value = dto.Status;

        command.Parameters.Add(
            "@JurisdiccionId",
            SqlDbType.Int).Value =
            dto.JurisdictionId;

        command.Parameters.Add(
            "@FechaInstalacion",
            SqlDbType.DateTime2).Value =
            dto.InstallationDate.HasValue
                ? dto.InstallationDate.Value
                : DBNull.Value;

        await connection.OpenAsync(
            cancellationToken);

        var result =
            await command.ExecuteScalarAsync(
                cancellationToken);

        return Convert.ToInt32(result);
    }

    private static AssetDto MapReaderToDto(
        SqlDataReader reader)
    {
        return new AssetDto
        {
            Id =
                reader.GetInt32(
                    reader.GetOrdinal("Id")),

            Code =
                reader.GetString(
                    reader.GetOrdinal("Codigo")),

            Name =
                reader.GetString(
                    reader.GetOrdinal("Nombre")),

            Type =
                reader.GetString(
                    reader.GetOrdinal("Tipo")),

            Status =
                reader.GetString(
                    reader.GetOrdinal("Estado")),

            JurisdictionId =
                reader.GetInt32(
                    reader.GetOrdinal(
                        "JurisdiccionId")),

            JurisdictionName =
                reader.GetString(
                    reader.GetOrdinal(
                        "NombreJurisdiccion")),

            InstallationDate =
                GetNullableDateTime(
                    reader,
                    "FechaInstalacion"),

            IsActive =
                reader.GetBoolean(
                    reader.GetOrdinal("Activo"))
        };
    }

    private static DateTime? GetNullableDateTime(
        SqlDataReader reader,
        string columnName)
    {
        var ordinal =
            reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetDateTime(ordinal);
    }
}