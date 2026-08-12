using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Report;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories
{
    public sealed class ReportRepository : IReportRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public ReportRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<ReportDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default)
        {
            var list = new List<ReportDto>();
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT r.Id, r.IncidenciaId, r.TrabajoId, r.GeneradoPorId, u.NombreUsuario AS NombreUsuarioGenerador,
                   r.Contenido, r.RutaArchivo, r.FechaGeneracion
            FROM dbo.Reportes r
            INNER JOIN dbo.Usuarios u ON r.GeneradoPorId = u.Id
            WHERE r.IncidenciaId = @IncidentId
            ORDER BY r.FechaGeneracion DESC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@IncidentId", incidentId);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<ReportDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT r.Id, r.IncidenciaId, r.TrabajoId, r.GeneradoPorId, u.NombreUsuario AS NombreUsuarioGenerador,
                   r.Contenido, r.RutaArchivo, r.FechaGeneracion
            FROM dbo.Reportes r
            INNER JOIN dbo.Usuarios u ON r.GeneradoPorId = u.Id
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
            CreateReportDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            INSERT INTO dbo.Reportes (IncidenciaId, TrabajoId, GeneradoPorId, Contenido, RutaArchivo)
            OUTPUT INSERTED.Id
            VALUES (@IncidenciaId, @TrabajoId, @GeneradoPorId, @Contenido, @RutaArchivo);";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@IncidenciaId", dto.IncidentId);
            cmd.Parameters.AddWithValue("@TrabajoId", (object?)dto.JobId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GeneradoPorId", dto.GeneratedByUserId);
            cmd.Parameters.AddWithValue("@Contenido", (object?)dto.Content ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RutaArchivo", (object?)dto.FilePath ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        private static ReportDto MapReaderToDto(SqlDataReader reader)
        {
            return new ReportDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                IncidentId = reader.GetInt32(reader.GetOrdinal("IncidenciaId")),
                JobId = reader.IsDBNull(reader.GetOrdinal("TrabajoId")) ? null : reader.GetInt32(reader.GetOrdinal("TrabajoId")),
                GeneratedByUserId = reader.GetInt32(reader.GetOrdinal("GeneradoPorId")),
                GeneratedByUserName = reader.GetString(reader.GetOrdinal("NombreUsuarioGenerador")),
                Content = reader.IsDBNull(reader.GetOrdinal("Contenido")) ? null : reader.GetString(reader.GetOrdinal("Contenido")),
                FilePath = reader.IsDBNull(reader.GetOrdinal("RutaArchivo")) ? null : reader.GetString(reader.GetOrdinal("RutaArchivo")),
                GeneratedAt = reader.GetDateTime(reader.GetOrdinal("FechaGeneracion"))
            };
        }
    }
}
