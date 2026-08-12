using Microsoft.Data.SqlClient;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Jurisdiction;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories
{
    public sealed class JurisdictionRepository : IJurisdictionRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public JurisdictionRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IReadOnlyList<JurisdictionDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var list = new List<JurisdictionDto>();
            using var conn = _dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT j.Id, j.Nombre, j.Nivel, j.JurisdiccionPadreId, 
                   padre.Nombre AS NombrePadre, j.Activo
            FROM dbo.Jurisdicciones j
            LEFT JOIN dbo.Jurisdicciones padre ON j.JurisdiccionPadreId = padre.Id
            WHERE j.Activo = 1
            ORDER BY j.Nombre ASC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<JurisdictionDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT j.Id, j.Nombre, j.Nivel, j.JurisdiccionPadreId, 
                   padre.Nombre AS NombrePadre, j.Activo
            FROM dbo.Jurisdicciones j
            LEFT JOIN dbo.Jurisdicciones padre ON j.JurisdiccionPadreId = padre.Id
            WHERE j.Id = @Id;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapReaderToDto(reader);
            }

            return null;
        }

        public async Task<int> CreateAsync(
            CreateJurisdictionDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            INSERT INTO dbo.Jurisdicciones (Nombre, Nivel, JurisdiccionPadreId)
            OUTPUT INSERTED.Id
            VALUES (@Nombre, @Nivel, @JurisdiccionPadreId);";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Nombre", dto.Name);
            cmd.Parameters.AddWithValue("@Nivel", dto.Level);
            cmd.Parameters.AddWithValue("@JurisdiccionPadreId", (object?)dto.ParentJurisdictionId ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        private static JurisdictionDto MapReaderToDto(SqlDataReader reader)
        {
            return new JurisdictionDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Nombre")),
                Level = reader.GetString(reader.GetOrdinal("Nivel")),
                ParentJurisdictionId = reader.IsDBNull(reader.GetOrdinal("JurisdiccionPadreId")) ? null : reader.GetInt32(reader.GetOrdinal("JurisdiccionPadreId")),
                ParentJurisdictionName = reader.IsDBNull(reader.GetOrdinal("NombrePadre")) ? null : reader.GetString(reader.GetOrdinal("NombrePadre")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("Activo"))
            };
        }
    }
}
