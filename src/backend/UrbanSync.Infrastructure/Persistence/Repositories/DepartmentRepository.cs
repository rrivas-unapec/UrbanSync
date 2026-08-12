using Microsoft.Data.SqlClient;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Departament;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories
{
    public sealed class DepartmentRepository : IDepartmentRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public DepartmentRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var list = new List<DepartmentDto>();
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT d.Id, d.Nombre, d.JurisdiccionId, j.Nombre AS NombreJurisdiccion, d.Activo
            FROM dbo.Departamentos d
            LEFT JOIN dbo.Jurisdicciones j ON d.JurisdiccionId = j.Id
            WHERE d.Activo = 1
            ORDER BY d.Nombre ASC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<DepartmentDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT d.Id, d.Nombre, d.JurisdiccionId, j.Nombre AS NombreJurisdiccion, d.Activo
            FROM dbo.Departamentos d
            LEFT JOIN dbo.Jurisdicciones j ON d.JurisdiccionId = j.Id
            WHERE d.Id = @Id;";

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
            CreateDepartmentDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            INSERT INTO dbo.Departamentos (Nombre, JurisdiccionId)
            OUTPUT INSERTED.Id
            VALUES (@Nombre, @JurisdiccionId);";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Nombre", dto.Name);
            cmd.Parameters.AddWithValue("@JurisdiccionId", (object?)dto.JurisdictionId ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        private static DepartmentDto MapReaderToDto(SqlDataReader reader)
        {
            return new DepartmentDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Nombre")),
                JurisdictionId = reader.IsDBNull(reader.GetOrdinal("JurisdiccionId")) ? null : reader.GetInt32(reader.GetOrdinal("JurisdiccionId")),
                JurisdictionName = reader.IsDBNull(reader.GetOrdinal("NombreJurisdiccion")) ? null : reader.GetString(reader.GetOrdinal("NombreJurisdiccion")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("Activo"))
            };
        }
    }
}
