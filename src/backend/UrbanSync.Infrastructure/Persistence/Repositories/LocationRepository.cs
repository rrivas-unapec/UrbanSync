using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Location;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories
{
    public sealed class LocationRepository : ILocationRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public LocationRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<LocationDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var list = new List<LocationDto>();
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT u.Id, u.Direccion, u.Referencia, u.Latitud, u.Longitud, 
                   u.JurisdiccionId, j.Nombre AS NombreJurisdiccion, u.FechaCreacion
            FROM dbo.Ubicaciones u
            INNER JOIN dbo.Jurisdicciones j ON u.JurisdiccionId = j.Id
            ORDER BY u.Id DESC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<LocationDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT u.Id, u.Direccion, u.Referencia, u.Latitud, u.Longitud, 
                   u.JurisdiccionId, j.Nombre AS NombreJurisdiccion, u.FechaCreacion
            FROM dbo.Ubicaciones u
            INNER JOIN dbo.Jurisdicciones j ON u.JurisdiccionId = j.Id
            WHERE u.Id = @Id;";

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
            CreateLocationDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            INSERT INTO dbo.Ubicaciones (Direccion, Referencia, Latitud, Longitud, JurisdiccionId)
            OUTPUT INSERTED.Id
            VALUES (@Direccion, @Referencia, @Latitud, @Longitud, @JurisdiccionId);";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Direccion", dto.Address);
            cmd.Parameters.AddWithValue("@Referencia", (object?)dto.Reference ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Latitud", (object?)dto.Latitude ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Longitud", (object?)dto.Longitude ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@JurisdiccionId", dto.JurisdictionId);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        private static LocationDto MapReaderToDto(SqlDataReader reader)
        {
            return new LocationDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Address = reader.GetString(reader.GetOrdinal("Direccion")),
                Reference = reader.IsDBNull(reader.GetOrdinal("Referencia")) ? null : reader.GetString(reader.GetOrdinal("Referencia")),
                Latitude = reader.IsDBNull(reader.GetOrdinal("Latitud")) ? null : reader.GetDecimal(reader.GetOrdinal("Latitud")),
                Longitude = reader.IsDBNull(reader.GetOrdinal("Longitud")) ? null : reader.GetDecimal(reader.GetOrdinal("Longitud")),
                JurisdictionId = reader.GetInt32(reader.GetOrdinal("JurisdiccionId")),
                JurisdictionName = reader.GetString(reader.GetOrdinal("NombreJurisdiccion")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
            };
        }
    }
}
