using Microsoft.Data.SqlClient;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Asset;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories
{
    public sealed class AssetRepository : IAssetRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AssetRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<AssetDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var list = new List<AssetDto>();
            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT a.Id, a.Codigo, a.Nombre, a.Tipo, a.Estado, a.JurisdiccionId, 
                   j.Nombre AS NombreJurisdiccion, a.FechaInstalacion, a.Activo
            FROM dbo.Activos a
            INNER JOIN dbo.Jurisdicciones j ON a.JurisdiccionId = j.Id
            WHERE a.Activo = 1
            ORDER BY a.Id DESC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<AssetDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT a.Id, a.Codigo, a.Nombre, a.Tipo, a.Estado, a.JurisdiccionId, 
                   j.Nombre AS NombreJurisdiccion, a.FechaInstalacion, a.Activo
            FROM dbo.Activos a
            INNER JOIN dbo.Jurisdicciones j ON a.JurisdiccionId = j.Id
            WHERE a.Id = @Id;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapReaderToDto(reader);
            }

            return null;
        }

        public async Task<AssetHistoryDto?> GetHistoryByIdAsync(
         int id,
         CancellationToken cancellationToken = default)
            {
                using var conn = _connectionFactory.CreateConnection();
                await conn.OpenAsync(cancellationToken);

                const string query = @"
            SELECT TOP 1 i.Id AS IncidentId, i.CodigoCaso, ti.Nombre AS IncidentType, 
                   i.Descripcion, i.Estado, i.FechaReporte
            FROM dbo.Incidencias i
            INNER JOIN dbo.TiposIncidencia ti ON i.TipoIncidenciaId = ti.Id
            INNER JOIN dbo.Ubicaciones u ON i.UbicacionId = u.Id
            INNER JOIN dbo.Activos a ON a.JurisdiccionId = u.JurisdiccionId
            WHERE a.Id = @AssetId
            ORDER BY i.FechaReporte DESC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@AssetId", id);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new AssetHistoryDto
                {
                    IncidentId = reader.GetInt32(reader.GetOrdinal("IncidentId")),
                    CaseCode = reader.GetString(reader.GetOrdinal("CodigoCaso")),
                    IncidentType = reader.GetString(reader.GetOrdinal("IncidentType")),
                    Description = reader.GetString(reader.GetOrdinal("Descripcion")),
                    Status = reader.GetString(reader.GetOrdinal("Estado")),
                    ReportDate = reader.GetDateTime(reader.GetOrdinal("FechaReporte"))
                };
            }

            return null;
        }

        public async Task<int> CreateAsync(
            CreateAssetDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            INSERT INTO dbo.Activos (Codigo, Nombre, Tipo, Estado, JurisdiccionId, FechaInstalacion)
            OUTPUT INSERTED.Id
            VALUES (@Codigo, @Nombre, @Tipo, @Estado, @JurisdiccionId, ISNULL(@FechaInstalacion, SYSDATETIME()));";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Codigo", dto.Code);
            cmd.Parameters.AddWithValue("@Nombre", dto.Name);
            cmd.Parameters.AddWithValue("@Tipo", dto.Type);
            cmd.Parameters.AddWithValue("@Estado", dto.Status);
            cmd.Parameters.AddWithValue("@JurisdiccionId", dto.JurisdictionId);
            cmd.Parameters.AddWithValue("@FechaInstalacion", (object?)dto.InstallationDate ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        private static AssetDto MapReaderToDto(SqlDataReader reader)
        {
            return new AssetDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Code = reader.GetString(reader.GetOrdinal("Codigo")),
                Name = reader.GetString(reader.GetOrdinal("Nombre")),
                Type = reader.GetString(reader.GetOrdinal("Tipo")),
                Status = reader.GetString(reader.GetOrdinal("Estado")),
                JurisdictionId = reader.GetInt32(reader.GetOrdinal("JurisdiccionId")),
                JurisdictionName = reader.GetString(reader.GetOrdinal("NombreJurisdiccion")),
                InstallationDate = reader.IsDBNull(reader.GetOrdinal("FechaInstalacion")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaInstalacion")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("Activo"))
            };
        }
    }
}