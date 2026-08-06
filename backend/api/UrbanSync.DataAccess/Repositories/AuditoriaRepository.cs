using Microsoft.Data.SqlClient;
using UrbanSync.Domain.DTOs;

namespace UrbanSync.DataAccess.Repositories
{
    public class AuditoriaRepository : IAuditoriaRepository
    {
        private readonly DbConnectionFactory _dbFactory;

        public AuditoriaRepository(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<AuditoriaDto>> GetAllAsync(AuditoriaFilterDto filter)
        {
            var list = new List<AuditoriaDto>();
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync();

            var query = @"
            SELECT a.Id, a.UsuarioId, u.NombreUsuario, a.Accion, a.Entidad, a.EntidadId,
                   a.Detalle, a.IpOrigen, a.FechaHora
            FROM dbo.AuditoriaAccesos a
            LEFT JOIN dbo.Usuarios u ON a.UsuarioId = u.Id
            WHERE 1=1";

            using var cmd = new SqlCommand();
            cmd.Connection = (SqlConnection)conn;

            if (filter.UsuarioId.HasValue)
            {
                query += " AND a.UsuarioId = @UsuarioId";
                cmd.Parameters.AddWithValue("@UsuarioId", filter.UsuarioId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Entidad))
            {
                query += " AND a.Entidad = @Entidad";
                cmd.Parameters.AddWithValue("@Entidad", filter.Entidad);
            }

            if (!string.IsNullOrWhiteSpace(filter.Accion))
            {
                query += " AND a.Accion = @Accion";
                cmd.Parameters.AddWithValue("@Accion", filter.Accion);
            }

            if (filter.FechaInicio.HasValue)
            {
                query += " AND a.FechaHora >= @FechaInicio";
                cmd.Parameters.AddWithValue("@FechaInicio", filter.FechaInicio.Value);
            }

            if (filter.FechaFin.HasValue)
            {
                query += " AND a.FechaHora <= @FechaFin";
                cmd.Parameters.AddWithValue("@FechaFin", filter.FechaFin.Value);
            }

            query += " ORDER BY a.FechaHora DESC;";
            cmd.CommandText = query;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<AuditoriaDto?> GetByIdAsync(long id)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync();

            var query = @"
            SELECT a.Id, a.UsuarioId, u.NombreUsuario, a.Accion, a.Entidad, a.EntidadId,
                   a.Detalle, a.IpOrigen, a.FechaHora
            FROM dbo.AuditoriaAccesos a
            LEFT JOIN dbo.Usuarios u ON a.UsuarioId = u.Id
            WHERE a.Id = @Id;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToDto(reader);
            }

            return null;
        }

        public async Task<long> CreateAsync(AuditoriaCreateDto dto)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync();

            var query = @"
            INSERT INTO dbo.AuditoriaAccesos (UsuarioId, Accion, Entidad, EntidadId, Detalle, IpOrigen)
            OUTPUT INSERTED.Id
            VALUES (@UsuarioId, @Accion, @Entidad, @EntidadId, @Detalle, @IpOrigen);";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@UsuarioId", (object?)dto.UsuarioId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Accion", dto.Accion);
            cmd.Parameters.AddWithValue("@Entidad", (object?)dto.Entidad ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EntidadId", (object?)dto.EntidadId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Detalle", (object?)dto.Detalle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IpOrigen", (object?)dto.IpOrigen ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        private static AuditoriaDto MapReaderToDto(SqlDataReader reader)
        {
            return new AuditoriaDto
            {
                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                UsuarioId = reader.IsDBNull(reader.GetOrdinal("UsuarioId")) ? null : reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                NombreUsuario = reader.IsDBNull(reader.GetOrdinal("NombreUsuario")) ? null : reader.GetString(reader.GetOrdinal("NombreUsuario")),
                Accion = reader.GetString(reader.GetOrdinal("Accion")),
                Entidad = reader.IsDBNull(reader.GetOrdinal("Entidad")) ? null : reader.GetString(reader.GetOrdinal("Entidad")),
                EntidadId = reader.IsDBNull(reader.GetOrdinal("EntidadId")) ? null : reader.GetInt32(reader.GetOrdinal("EntidadId")),
                Detalle = reader.IsDBNull(reader.GetOrdinal("Detalle")) ? null : reader.GetString(reader.GetOrdinal("Detalle")),
                IpOrigen = reader.IsDBNull(reader.GetOrdinal("IpOrigen")) ? null : reader.GetString(reader.GetOrdinal("IpOrigen")),
                FechaHora = reader.GetDateTime(reader.GetOrdinal("FechaHora"))
            };
        }
    }
}
