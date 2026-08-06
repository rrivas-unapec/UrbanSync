using Microsoft.Data.SqlClient;
using UrbanSync.Domain.DTOs;

namespace UrbanSync.DataAccess.Repositories
{
    public class IncidenciaRepository : IIncidenciaRepository
    {
        private readonly DbConnectionFactory _dbFactory;

        public IncidenciaRepository(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<IncidenciaDto>> GetAllAsync()
        {
            var list = new List<IncidenciaDto>();
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync();

            string query = @"
            SELECT i.Id, i.CodigoCaso, i.UsuarioReportaId, u.NombreCompleto AS NombreUsuarioReporta,
                   i.TipoIncidenciaId, ti.Nombre AS NombreTipoIncidencia, i.UbicacionId, ub.Direccion AS DireccionUbicacion,
                   i.InstitucionAsignadaId, inst.Nombre AS NombreInstitucionAsignada, i.Estado, i.Prioridad,
                   i.Descripcion, i.FechaReporte, i.FechaAsignacion, i.FechaCierre
            FROM dbo.Incidencias i
            INNER JOIN dbo.Usuarios u ON i.UsuarioReportaId = u.Id
            INNER JOIN dbo.TiposIncidencia ti ON i.TipoIncidenciaId = ti.Id
            INNER JOIN dbo.Ubicaciones ub ON i.UbicacionId = ub.Id
            LEFT JOIN dbo.Instituciones inst ON i.InstitucionAsignadaId = inst.Id
            ORDER BY i.FechaReporte DESC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapReaderToDto(reader));
            }
            return list;
        }

        public async Task<IncidenciaDto?> GetByIdAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync();

            string query = @"
            SELECT i.Id, i.CodigoCaso, i.UsuarioReportaId, u.NombreCompleto AS NombreUsuarioReporta,
                   i.TipoIncidenciaId, ti.Nombre AS NombreTipoIncidencia, i.UbicacionId, ub.Direccion AS DireccionUbicacion,
                   i.InstitucionAsignadaId, inst.Nombre AS NombreInstitucionAsignada, i.Estado, i.Prioridad,
                   i.Descripcion, i.FechaReporte, i.FechaAsignacion, i.FechaCierre
            FROM dbo.Incidencias i
            INNER JOIN dbo.Usuarios u ON i.UsuarioReportaId = u.Id
            INNER JOIN dbo.TiposIncidencia ti ON i.TipoIncidenciaId = ti.Id
            INNER JOIN dbo.Ubicaciones ub ON i.UbicacionId = ub.Id
            LEFT JOIN dbo.Instituciones inst ON i.InstitucionAsignadaId = inst.Id
            WHERE i.Id = @Id;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToDto(reader);
            }
            return null;
        }

        public async Task<int> CreateAsync(IncidenciaCreateDto dto, string codigoCaso)
        {
            using var conn = (SqlConnection)_dbFactory.CreateConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                string getInstQuery = "SELECT InstitucionId FROM dbo.TiposIncidencia WHERE Id = @TipoId;";
                using var cmdInst = new SqlCommand(getInstQuery, conn, transaction);
                cmdInst.Parameters.AddWithValue("@TipoId", dto.TipoIncidenciaId);
                var instResult = await cmdInst.ExecuteScalarAsync();
                int? institucionId = instResult != null ? Convert.ToInt32(instResult) : null;

                string insertUbicacion = @"
                INSERT INTO dbo.Ubicaciones (Direccion, Referencia, Latitud, Longitud, JurisdiccionId)
                OUTPUT INSERTED.Id
                VALUES (@Direccion, @Referencia, @Latitud, @Longitud, @JurisdiccionId);";

                using var cmdUbicacion = new SqlCommand(insertUbicacion, conn, transaction);
                cmdUbicacion.Parameters.AddWithValue("@Direccion", dto.Direccion);
                cmdUbicacion.Parameters.AddWithValue("@Referencia", (object?)dto.Referencia ?? DBNull.Value);
                cmdUbicacion.Parameters.AddWithValue("@Latitud", (object?)dto.Latitud ?? DBNull.Value);
                cmdUbicacion.Parameters.AddWithValue("@Longitud", (object?)dto.Longitud ?? DBNull.Value);
                cmdUbicacion.Parameters.AddWithValue("@JurisdiccionId", dto.JurisdiccionId);

                int ubicacionId = (int)await cmdUbicacion.ExecuteScalarAsync();

                // 3. Insertar Incidencia
                string insertIncidencia = @"
                INSERT INTO dbo.Incidencias (CodigoCaso, UsuarioReportaId, TipoIncidenciaId, UbicacionId, InstitucionAsignadaId, Prioridad, Descripcion)
                OUTPUT INSERTED.Id
                VALUES (@CodigoCaso, @UsuarioReportaId, @TipoIncidenciaId, @UbicacionId, @InstitucionAsignadaId, @Prioridad, @Descripcion);";

                using var cmdIncidencia = new SqlCommand(insertIncidencia, conn, transaction);
                cmdIncidencia.Parameters.AddWithValue("@CodigoCaso", codigoCaso);
                cmdIncidencia.Parameters.AddWithValue("@UsuarioReportaId", dto.UsuarioReportaId);
                cmdIncidencia.Parameters.AddWithValue("@TipoIncidenciaId", dto.TipoIncidenciaId);
                cmdIncidencia.Parameters.AddWithValue("@UbicacionId", ubicacionId);
                cmdIncidencia.Parameters.AddWithValue("@InstitucionAsignadaId", (object?)institucionId ?? DBNull.Value);
                cmdIncidencia.Parameters.AddWithValue("@Prioridad", dto.Prioridad);
                cmdIncidencia.Parameters.AddWithValue("@Descripcion", dto.Descripcion);

                int incidenciaId = (int)await cmdIncidencia.ExecuteScalarAsync();

                transaction.Commit();
                return incidenciaId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateEstadoAsync(int id, string estado, int? institucionId)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync();

            string query = @"
            UPDATE dbo.Incidencias
            SET Estado = @Estado,
                InstitucionAsignadaId = ISNULL(@InstitucionId, InstitucionAsignadaId),
                FechaAsignacion = CASE WHEN @Estado = 'Asignada' AND FechaAsignacion IS NULL THEN SYSDATETIME() ELSE FechaAsignacion END,
                FechaCierre = CASE WHEN @Estado IN ('Cerrada', 'Rechazada') THEN SYSDATETIME() ELSE FechaCierre END
            WHERE Id = @Id;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Estado", estado);
            cmd.Parameters.AddWithValue("@InstitucionId", (object?)institucionId ?? DBNull.Value);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        private static IncidenciaDto MapReaderToDto(SqlDataReader reader)
        {
            return new IncidenciaDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CodigoCaso = reader.GetString(reader.GetOrdinal("CodigoCaso")),
                UsuarioReportaId = reader.GetInt32(reader.GetOrdinal("UsuarioReportaId")),
                NombreUsuarioReporta = reader.GetString(reader.GetOrdinal("NombreUsuarioReporta")),
                TipoIncidenciaId = reader.GetInt32(reader.GetOrdinal("TipoIncidenciaId")),
                NombreTipoIncidencia = reader.GetString(reader.GetOrdinal("NombreTipoIncidencia")),
                UbicacionId = reader.GetInt32(reader.GetOrdinal("UbicacionId")),
                DireccionUbicacion = reader.GetString(reader.GetOrdinal("DireccionUbicacion")),
                InstitucionAsignadaId = reader.IsDBNull(reader.GetOrdinal("InstitucionAsignadaId")) ? null : reader.GetInt32(reader.GetOrdinal("InstitucionAsignadaId")),
                NombreInstitucionAsignada = reader.IsDBNull(reader.GetOrdinal("NombreInstitucionAsignada")) ? null : reader.GetString(reader.GetOrdinal("NombreInstitucionAsignada")),
                Estado = reader.GetString(reader.GetOrdinal("Estado")),
                Prioridad = reader.GetString(reader.GetOrdinal("Prioridad")),
                Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
                FechaReporte = reader.GetDateTime(reader.GetOrdinal("FechaReporte")),
                FechaAsignacion = reader.IsDBNull(reader.GetOrdinal("FechaAsignacion")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaAsignacion")),
                FechaCierre = reader.IsDBNull(reader.GetOrdinal("FechaCierre")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaCierre"))
            };
        }
    }
}
