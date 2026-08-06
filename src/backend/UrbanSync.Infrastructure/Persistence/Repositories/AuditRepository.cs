using System.Data;
using Microsoft.Data.SqlClient;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Audit;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories;

public sealed class AuditRepository : IAuditRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuditRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AuditDto>> GetAllAsync(
        AuditFilterDto? filter = null,
        CancellationToken cancellationToken = default)
    {
        var audits = new List<AuditDto>();

        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            """
            SELECT
                a.Id,
                a.UsuarioId,
                u.NombreUsuario,
                a.Accion,
                a.Entidad,
                a.EntidadId,
                a.Detalle,
                a.IpOrigen,
                a.FechaHora
            FROM dbo.AuditoriaAccesos AS a
            LEFT JOIN dbo.Usuarios AS u
                ON u.Id = a.UsuarioId
            WHERE
                (@UsuarioId IS NULL OR a.UsuarioId = @UsuarioId)
                AND (@Entidad IS NULL OR a.Entidad = @Entidad)
                AND (@Accion IS NULL OR a.Accion = @Accion)
                AND (@FechaInicio IS NULL OR a.FechaHora >= @FechaInicio)
                AND (@FechaFin IS NULL OR a.FechaHora <= @FechaFin)
            ORDER BY a.FechaHora DESC;
            """,
            connection);

        AddNullableInt(
            command,
            "@UsuarioId",
            filter?.UserId);

        AddNullableString(
            command,
            "@Entidad",
            Normalize(filter?.Entity),
            80);

        AddNullableString(
            command,
            "@Accion",
            Normalize(filter?.Action),
            50);

        AddNullableDateTime(
            command,
            "@FechaInicio",
            filter?.StartDate);

        AddNullableDateTime(
            command,
            "@FechaFin",
            filter?.EndDate);

        await connection.OpenAsync(cancellationToken);

        using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            audits.Add(Map(reader));
        }

        return audits;
    }

    public async Task<AuditDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            """
            SELECT
                a.Id,
                a.UsuarioId,
                u.NombreUsuario,
                a.Accion,
                a.Entidad,
                a.EntidadId,
                a.Detalle,
                a.IpOrigen,
                a.FechaHora
            FROM dbo.AuditoriaAccesos AS a
            LEFT JOIN dbo.Usuarios AS u
                ON u.Id = a.UsuarioId
            WHERE a.Id = @Id;
            """,
            connection);

        command.Parameters
            .Add("@Id", SqlDbType.BigInt)
            .Value = id;

        await connection.OpenAsync(cancellationToken);

        using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? Map(reader)
            : null;
    }

    public async Task<long> CreateAsync(
        CreateAuditDto audit,
        CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            """
            INSERT INTO dbo.AuditoriaAccesos
            (
                UsuarioId,
                Accion,
                Entidad,
                EntidadId,
                Detalle,
                IpOrigen
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @UsuarioId,
                @Accion,
                @Entidad,
                @EntidadId,
                @Detalle,
                @IpOrigen
            );
            """,
            connection);

        AddNullableInt(
            command,
            "@UsuarioId",
            audit.UserId);

        command.Parameters
            .Add(
                "@Accion",
                SqlDbType.NVarChar,
                50)
            .Value = audit.Action;

        AddNullableString(
            command,
            "@Entidad",
            audit.Entity,
            80);

        AddNullableInt(
            command,
            "@EntidadId",
            audit.EntityId);

        AddNullableString(
            command,
            "@Detalle",
            audit.Detail,
            400);

        AddNullableString(
            command,
            "@IpOrigen",
            audit.IpAddress,
            45);

        await connection.OpenAsync(cancellationToken);

        var result =
            await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt64(result);
    }

    private static AuditDto Map(
        SqlDataReader reader)
    {
        return new AuditDto
        {
            Id = reader.GetInt64(
                reader.GetOrdinal("Id")),

            UserId = GetNullableInt(
                reader,
                "UsuarioId"),

            UserName = GetNullableString(
                reader,
                "NombreUsuario"),

            Action = reader.GetString(
                reader.GetOrdinal("Accion")),

            Entity = GetNullableString(
                reader,
                "Entidad"),

            EntityId = GetNullableInt(
                reader,
                "EntidadId"),

            Detail = GetNullableString(
                reader,
                "Detalle"),

            IpAddress = GetNullableString(
                reader,
                "IpOrigen"),

            Timestamp = reader.GetDateTime(
                reader.GetOrdinal("FechaHora"))
        };
    }

    private static string? Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static void AddNullableInt(
        SqlCommand command,
        string parameter,
        int? value)
    {
        command.Parameters
            .Add(parameter, SqlDbType.Int)
            .Value = value ?? (object)DBNull.Value;
    }

    private static void AddNullableDateTime(
        SqlCommand command,
        string parameter,
        DateTime? value)
    {
        command.Parameters
            .Add(parameter, SqlDbType.DateTime2)
            .Value = value ?? (object)DBNull.Value;
    }

    private static void AddNullableString(
        SqlCommand command,
        string parameter,
        string? value,
        int size)
    {
        command.Parameters
            .Add(
                parameter,
                SqlDbType.NVarChar,
                size)
            .Value = string.IsNullOrWhiteSpace(value)
                ? DBNull.Value
                : value;
    }

    private static int? GetNullableInt(
        SqlDataReader reader,
        string column)
    {
        var ordinal = reader.GetOrdinal(column);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetInt32(ordinal);
    }

    private static string? GetNullableString(
        SqlDataReader reader,
        string column)
    {
        var ordinal = reader.GetOrdinal(column);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetString(ordinal);
    }
}