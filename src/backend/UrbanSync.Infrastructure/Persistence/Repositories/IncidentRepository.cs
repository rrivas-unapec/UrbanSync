using System.Data;
using Microsoft.Data.SqlClient;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Incidents;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories;

public sealed class IncidentRepository : IIncidentRepository
{
    private const string SelectIncident =
        """
        SELECT
            i.Id,
            i.CodigoCaso,
            i.UsuarioReportaId,
            u.NombreCompleto AS UsuarioReporta,
            i.TipoIncidenciaId,
            ti.Nombre AS TipoIncidencia,
            i.UbicacionId,
            ub.Direccion,
            ub.Referencia,
            ub.Latitud,
            ub.Longitud,
            ub.JurisdiccionId,
            j.Nombre AS Jurisdiccion,
            i.InstitucionAsignadaId,
            inst.Nombre AS InstitucionAsignada,
            i.Estado,
            i.Prioridad,
            i.Descripcion,
            i.FechaReporte,
            i.FechaAsignacion,
            i.FechaCierre
        FROM dbo.Incidencias i
        INNER JOIN dbo.Usuarios u
            ON i.UsuarioReportaId = u.Id
        INNER JOIN dbo.TiposIncidencia ti
            ON i.TipoIncidenciaId = ti.Id
        INNER JOIN dbo.Ubicaciones ub
            ON i.UbicacionId = ub.Id
        INNER JOIN dbo.Jurisdicciones j
            ON ub.JurisdiccionId = j.Id
        LEFT JOIN dbo.Instituciones inst
            ON i.InstitucionAsignadaId = inst.Id
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public IncidentRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<IncidentDto>> GetAllAsync(
        string? status = null,
        int? reportingUserId = null,
        CancellationToken cancellationToken = default)
    {
        var incidents = new List<IncidentDto>();

        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            $"""
            {SelectIncident}
            WHERE
                (@Status IS NULL OR i.Estado = @Status)
                AND
                (
                    @ReportingUserId IS NULL
                    OR i.UsuarioReportaId = @ReportingUserId
                )
            ORDER BY i.FechaReporte DESC;
            """,
            connection);

        AddNullableNVarChar(
            command,
            "@Status",
            status,
            30);

        AddNullableInt(
            command,
            "@ReportingUserId",
            reportingUserId);

        await connection.OpenAsync(cancellationToken);

        using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            incidents.Add(Map(reader));
        }

        return incidents;
    }

    public async Task<IncidentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            $"""
            {SelectIncident}
            WHERE i.Id = @Id;
            """,
            connection);

        command.Parameters.Add(
            "@Id",
            SqlDbType.Int).Value = id;

        await connection.OpenAsync(cancellationToken);

        using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? Map(reader)
            : null;
    }

    public async Task<int> CreateAsync(
        CreateIncidentDto incident,
        int reportingUserId,
        string caseCode,
        CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        using var transaction =
            connection.BeginTransaction();

        try
        {
            var assignedInstitutionId =
                await GetInstitutionForIncidentTypeAsync(
                    connection,
                    transaction,
                    incident.TipoIncidenciaId,
                    cancellationToken);

            var locationId =
                await CreateLocationAsync(
                    connection,
                    transaction,
                    incident,
                    cancellationToken);

            var incidentId =
                await CreateIncidentAsync(
                    connection,
                    transaction,
                    incident,
                    reportingUserId,
                    caseCode,
                    locationId,
                    assignedInstitutionId,
                    cancellationToken);

            transaction.Commit();

            return incidentId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(
        int id,
        string status,
        int? assignedInstitutionId,
        CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            """
            UPDATE dbo.Incidencias
            SET
                Estado = @Status,
                InstitucionAsignadaId =
                    COALESCE(
                        @AssignedInstitutionId,
                        InstitucionAsignadaId
                    ),
                FechaAsignacion =
                    CASE
                        WHEN
                            @Status = 'Asignada'
                            AND FechaAsignacion IS NULL
                        THEN SYSDATETIME()
                        ELSE FechaAsignacion
                    END,
                FechaCierre =
                    CASE
                        WHEN @Status IN ('Cerrada', 'Rechazada')
                        THEN SYSDATETIME()
                        WHEN @Status NOT IN ('Cerrada', 'Rechazada')
                        THEN NULL
                        ELSE FechaCierre
                    END
            WHERE Id = @Id;
            """,
            connection);

        command.Parameters.Add(
            "@Id",
            SqlDbType.Int).Value = id;

        command.Parameters.Add(
            "@Status",
            SqlDbType.NVarChar,
            30).Value = status;

        AddNullableInt(
            command,
            "@AssignedInstitutionId",
            assignedInstitutionId);

        await connection.OpenAsync(cancellationToken);

        return await command.ExecuteNonQueryAsync(
            cancellationToken) > 0;
    }

    public async Task<bool> TriageAsync(
        int id,
        TriageIncidentDto incident,
        string? resultingStatus,
        CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        using var transaction =
            connection.BeginTransaction();

        try
        {
            var currentData =
                await GetCurrentIncidentDataAsync(
                    connection,
                    transaction,
                    id,
                    cancellationToken);

            if (currentData is null)
            {
                transaction.Rollback();
                return false;
            }

            var resultingIncidentTypeId =
                incident.TipoIncidenciaId
                ?? currentData.Value.IncidentTypeId;

            var assignedInstitutionId =
                await GetInstitutionForIncidentTypeAsync(
                    connection,
                    transaction,
                    resultingIncidentTypeId,
                    cancellationToken);

            using var updateIncidentCommand = new SqlCommand(
                """
                UPDATE dbo.Incidencias
                SET
                    TipoIncidenciaId =
                        COALESCE(
                            @IncidentTypeId,
                            TipoIncidenciaId
                        ),
                    Prioridad =
                        COALESCE(
                            @Priority,
                            Prioridad
                        ),
                    Estado =
                        COALESCE(
                            @Status,
                            Estado
                        ),
                    InstitucionAsignadaId =
                        COALESCE(
                            @AssignedInstitutionId,
                            InstitucionAsignadaId
                        ),
                    FechaAsignacion =
                        CASE
                            WHEN
                                @Status = 'Asignada'
                                AND FechaAsignacion IS NULL
                            THEN SYSDATETIME()
                            ELSE FechaAsignacion
                        END,
                    FechaCierre =
                        CASE
                            WHEN @Status IN ('Cerrada', 'Rechazada')
                            THEN SYSDATETIME()
                            WHEN
                                @Status IS NOT NULL
                                AND @Status NOT IN (
                                    'Cerrada',
                                    'Rechazada'
                                )
                            THEN NULL
                            ELSE FechaCierre
                        END
                WHERE Id = @Id;
                """,
                connection,
                transaction);

            updateIncidentCommand.Parameters.Add(
                "@Id",
                SqlDbType.Int).Value = id;

            AddNullableInt(
                updateIncidentCommand,
                "@IncidentTypeId",
                incident.TipoIncidenciaId);

            AddNullableNVarChar(
                updateIncidentCommand,
                "@Priority",
                incident.Prioridad,
                20);

            AddNullableNVarChar(
                updateIncidentCommand,
                "@Status",
                resultingStatus,
                30);

            AddNullableInt(
                updateIncidentCommand,
                "@AssignedInstitutionId",
                assignedInstitutionId);

            var incidentRows =
                await updateIncidentCommand.ExecuteNonQueryAsync(
                    cancellationToken);

            if (incident.JurisdiccionId.HasValue)
            {
                using var updateLocationCommand =
                    new SqlCommand(
                        """
                        UPDATE dbo.Ubicaciones
                        SET JurisdiccionId = @JurisdictionId
                        WHERE Id = @LocationId;
                        """,
                        connection,
                        transaction);

                updateLocationCommand.Parameters.Add(
                    "@JurisdictionId",
                    SqlDbType.Int).Value =
                    incident.JurisdiccionId.Value;

                updateLocationCommand.Parameters.Add(
                    "@LocationId",
                    SqlDbType.Int).Value =
                    currentData.Value.LocationId;

                await updateLocationCommand.ExecuteNonQueryAsync(
                    cancellationToken);
            }

            transaction.Commit();

            return incidentRows > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static async Task<int?> GetInstitutionForIncidentTypeAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int incidentTypeId,
        CancellationToken cancellationToken)
    {
        using var command = new SqlCommand(
            """
            SELECT InstitucionId
            FROM dbo.TiposIncidencia
            WHERE
                Id = @IncidentTypeId
                AND Activo = 1;
            """,
            connection,
            transaction);

        command.Parameters.Add(
            "@IncidentTypeId",
            SqlDbType.Int).Value = incidentTypeId;

        var result =
            await command.ExecuteScalarAsync(cancellationToken);

        if (result is null || result == DBNull.Value)
        {
            throw new ArgumentException(
                $"No existe un tipo de incidencia activo con el ID {incidentTypeId}.");
        }

        return Convert.ToInt32(result);
    }

    private static async Task<int> CreateLocationAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        CreateIncidentDto incident,
        CancellationToken cancellationToken)
    {
        using var command = new SqlCommand(
            """
            INSERT INTO dbo.Ubicaciones
            (
                Direccion,
                Referencia,
                Latitud,
                Longitud,
                JurisdiccionId
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @Address,
                @Reference,
                @Latitude,
                @Longitude,
                @JurisdictionId
            );
            """,
            connection,
            transaction);

        command.Parameters.Add(
            "@Address",
            SqlDbType.NVarChar,
            250).Value = incident.Direccion;

        AddNullableNVarChar(
            command,
            "@Reference",
            incident.Referencia,
            250);

        AddNullableDecimal(
            command,
            "@Latitude",
            incident.Latitud,
            10,
            7);

        AddNullableDecimal(
            command,
            "@Longitude",
            incident.Longitud,
            10,
            7);

        command.Parameters.Add(
            "@JurisdictionId",
            SqlDbType.Int).Value =
            incident.JurisdiccionId;

        var result =
            await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(result);
    }

    private static async Task<int> CreateIncidentAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        CreateIncidentDto incident,
        int reportingUserId,
        string caseCode,
        int locationId,
        int? assignedInstitutionId,
        CancellationToken cancellationToken)
    {
        using var command = new SqlCommand(
            """
            INSERT INTO dbo.Incidencias
            (
                CodigoCaso,
                UsuarioReportaId,
                TipoIncidenciaId,
                UbicacionId,
                InstitucionAsignadaId,
                Estado,
                Prioridad,
                Descripcion
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @CaseCode,
                @ReportingUserId,
                @IncidentTypeId,
                @LocationId,
                @AssignedInstitutionId,
                'Registrada',
                @Priority,
                @Description
            );
            """,
            connection,
            transaction);

        command.Parameters.Add(
            "@CaseCode",
            SqlDbType.NVarChar,
            50).Value = caseCode;

        command.Parameters.Add(
            "@ReportingUserId",
            SqlDbType.Int).Value = reportingUserId;

        command.Parameters.Add(
            "@IncidentTypeId",
            SqlDbType.Int).Value =
            incident.TipoIncidenciaId;

        command.Parameters.Add(
            "@LocationId",
            SqlDbType.Int).Value = locationId;

        AddNullableInt(
            command,
            "@AssignedInstitutionId",
            assignedInstitutionId);

        command.Parameters.Add(
            "@Priority",
            SqlDbType.NVarChar,
            20).Value = incident.Prioridad;

        command.Parameters.Add(
            "@Description",
            SqlDbType.NVarChar,
            1000).Value = incident.Descripcion;

        var result =
            await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(result);
    }

    private static async Task<(int IncidentTypeId, int LocationId)?>
        GetCurrentIncidentDataAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            int id,
            CancellationToken cancellationToken)
    {
        using var command = new SqlCommand(
            """
            SELECT
                TipoIncidenciaId,
                UbicacionId
            FROM dbo.Incidencias
            WHERE Id = @Id;
            """,
            connection,
            transaction);

        command.Parameters.Add(
            "@Id",
            SqlDbType.Int).Value = id;

        using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return (
            reader.GetInt32(
                reader.GetOrdinal("TipoIncidenciaId")),
            reader.GetInt32(
                reader.GetOrdinal("UbicacionId"))
        );
    }

    private static IncidentDto Map(SqlDataReader reader)
    {
        return new IncidentDto
        {
            Id = reader.GetInt32(
                reader.GetOrdinal("Id")),

            CodigoCaso = reader.GetString(
                reader.GetOrdinal("CodigoCaso")),

            UsuarioReportaId = reader.GetInt32(
                reader.GetOrdinal("UsuarioReportaId")),

            UsuarioReporta = reader.GetString(
                reader.GetOrdinal("UsuarioReporta")),

            TipoIncidenciaId = reader.GetInt32(
                reader.GetOrdinal("TipoIncidenciaId")),

            TipoIncidencia = reader.GetString(
                reader.GetOrdinal("TipoIncidencia")),

            UbicacionId = reader.GetInt32(
                reader.GetOrdinal("UbicacionId")),

            Direccion = reader.GetString(
                reader.GetOrdinal("Direccion")),

            Referencia = GetNullableString(
                reader,
                "Referencia"),

            Latitud = GetNullableDecimal(
                reader,
                "Latitud"),

            Longitud = GetNullableDecimal(
                reader,
                "Longitud"),

            JurisdiccionId = reader.GetInt32(
                reader.GetOrdinal("JurisdiccionId")),

            Jurisdiccion = reader.GetString(
                reader.GetOrdinal("Jurisdiccion")),

            InstitucionAsignadaId = GetNullableInt(
                reader,
                "InstitucionAsignadaId"),

            InstitucionAsignada = GetNullableString(
                reader,
                "InstitucionAsignada"),

            Estado = reader.GetString(
                reader.GetOrdinal("Estado")),

            Prioridad = reader.GetString(
                reader.GetOrdinal("Prioridad")),

            Descripcion = reader.GetString(
                reader.GetOrdinal("Descripcion")),

            FechaReporte = reader.GetDateTime(
                reader.GetOrdinal("FechaReporte")),

            FechaAsignacion = GetNullableDateTime(
                reader,
                "FechaAsignacion"),

            FechaCierre = GetNullableDateTime(
                reader,
                "FechaCierre")
        };
    }

    private static void AddNullableInt(
        SqlCommand command,
        string parameterName,
        int? value)
    {
        command.Parameters.Add(
            parameterName,
            SqlDbType.Int).Value =
            value.HasValue
                ? value.Value
                : DBNull.Value;
    }

    private static void AddNullableNVarChar(
        SqlCommand command,
        string parameterName,
        string? value,
        int size)
    {
        command.Parameters.Add(
            parameterName,
            SqlDbType.NVarChar,
            size).Value =
            string.IsNullOrWhiteSpace(value)
                ? DBNull.Value
                : value;
    }

    private static void AddNullableDecimal(
        SqlCommand command,
        string parameterName,
        decimal? value,
        byte precision,
        byte scale)
    {
        var parameter = command.Parameters.Add(
            parameterName,
            SqlDbType.Decimal);

        parameter.Precision = precision;
        parameter.Scale = scale;
        parameter.Value =
            value.HasValue
                ? value.Value
                : DBNull.Value;
    }

    private static int? GetNullableInt(
        SqlDataReader reader,
        string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetInt32(ordinal);
    }

    private static decimal? GetNullableDecimal(
        SqlDataReader reader,
        string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetDecimal(ordinal);
    }

    private static string? GetNullableString(
        SqlDataReader reader,
        string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetString(ordinal);
    }

    private static DateTime? GetNullableDateTime(
        SqlDataReader reader,
        string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetDateTime(ordinal);
    }
}