using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Claim;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories
{
    public sealed class ClaimRepository : IClaimRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public ClaimRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<ClaimDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var list = new List<ClaimDto>();
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT r.Id, r.UsuarioCiudadanoId, u.NombreUsuario, 
                   r.UbicacionId, ub.Direccion AS DireccionUbicacion, 
                   r.Categoria, r.Titulo, r.Descripcion, r.Estado, r.FechaCreacion
            FROM dbo.Reclamaciones r
            INNER JOIN dbo.Usuarios u ON r.UsuarioCiudadanoId = u.Id
            INNER JOIN dbo.Ubicaciones ub ON r.UbicacionId = ub.Id
            ORDER BY r.FechaCreacion DESC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<IReadOnlyList<ClaimDto>> GetByCitizenIdAsync(
            int citizenUserId,
            CancellationToken cancellationToken = default)
        {
            var list = new List<ClaimDto>();
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT r.Id, r.UsuarioCiudadanoId, u.NombreUsuario, 
                   r.UbicacionId, ub.Direccion AS DireccionUbicacion, 
                   r.Categoria, r.Titulo, r.Descripcion, r.Estado, r.FechaCreacion
            FROM dbo.Reclamaciones r
            INNER JOIN dbo.Usuarios u ON r.UsuarioCiudadanoId = u.Id
            INNER JOIN dbo.Ubicaciones ub ON r.UbicacionId = ub.Id
            WHERE r.UsuarioCiudadanoId = @CitizenUserId
            ORDER BY r.FechaCreacion DESC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@CitizenUserId", citizenUserId);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<ClaimDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT r.Id, r.UsuarioCiudadanoId, u.NombreUsuario, 
                   r.UbicacionId, ub.Direccion AS DireccionUbicacion, 
                   r.Categoria, r.Titulo, r.Descripcion, r.Estado, r.FechaCreacion
            FROM dbo.Reclamaciones r
            INNER JOIN dbo.Usuarios u ON r.UsuarioCiudadanoId = u.Id
            INNER JOIN dbo.Ubicaciones ub ON r.UbicacionId = ub.Id
            WHERE r.Id = @Id;";

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
            CreateClaimDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            INSERT INTO dbo.Reclamaciones (UsuarioCiudadanoId, UbicacionId, Categoria, Titulo, Descripcion, Estado)
            OUTPUT INSERTED.Id
            VALUES (@UsuarioCiudadanoId, @UbicacionId, @Categoria, @Titulo, @Descripcion, 'Pendiente');";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@UsuarioCiudadanoId", dto.CitizenUserId);
            cmd.Parameters.AddWithValue("@UbicacionId", dto.LocationId);
            cmd.Parameters.AddWithValue("@Categoria", dto.Category);
            cmd.Parameters.AddWithValue("@Titulo", dto.Title);
            cmd.Parameters.AddWithValue("@Descripcion", dto.Description);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateStatusAsync(
            UpdateClaimStatusDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            UPDATE dbo.Reclamaciones
            SET Estado = @Estado
            WHERE Id = @Id;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Id", dto.Id);
            cmd.Parameters.AddWithValue("@Estado", dto.Status);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }

        private static ClaimDto MapReaderToDto(SqlDataReader reader)
        {
            return new ClaimDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CitizenUserId = reader.GetInt32(reader.GetOrdinal("UsuarioCiudadanoId")),
                CitizenUserName = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                LocationId = reader.GetInt32(reader.GetOrdinal("UbicacionId")),
                LocationAddress = reader.GetString(reader.GetOrdinal("DireccionUbicacion")),
                Category = reader.GetString(reader.GetOrdinal("Categoria")),
                Title = reader.GetString(reader.GetOrdinal("Titulo")),
                Description = reader.GetString(reader.GetOrdinal("Descripcion")),
                Status = reader.GetString(reader.GetOrdinal("Estado")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
            };
        }
    }
}
