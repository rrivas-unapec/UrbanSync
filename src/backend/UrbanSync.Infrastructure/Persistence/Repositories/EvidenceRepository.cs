using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Evidence;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories
{
    public sealed class EvidenceRepository : IEvidenceRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public EvidenceRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<EvidenceDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default)
        {
            var list = new List<EvidenceDto>();
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT e.Id, e.IncidenciaId, e.TipoEvidencia, e.RutaArchivo, e.Descripcion, 
                   e.UsuarioSubeId, u.NombreUsuario, e.FechaSubida
            FROM dbo.Evidencias e
            INNER JOIN dbo.Usuarios u ON e.UsuarioSubeId = u.Id
            WHERE e.IncidenciaId = @IncidentId
            ORDER BY e.FechaSubida DESC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@IncidentId", incidentId);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<EvidenceDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT e.Id, e.IncidenciaId, e.TipoEvidencia, e.RutaArchivo, e.Descripcion, 
                   e.UsuarioSubeId, u.NombreUsuario, e.FechaSubida
            FROM dbo.Evidencias e
            INNER JOIN dbo.Usuarios u ON e.UsuarioSubeId = u.Id
            WHERE e.Id = @Id;";

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
            CreateEvidenceDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            INSERT INTO dbo.Evidencias (IncidenciaId, TipoEvidencia, RutaArchivo, Descripcion, UsuarioSubeId)
            OUTPUT INSERTED.Id
            VALUES (@IncidenciaId, @TipoEvidencia, @RutaArchivo, @Descripcion, @UsuarioSubeId);";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@IncidenciaId", dto.IncidentId);
            cmd.Parameters.AddWithValue("@TipoEvidencia", dto.EvidenceType);
            cmd.Parameters.AddWithValue("@RutaArchivo", dto.FilePath);
            cmd.Parameters.AddWithValue("@Descripcion", (object?)dto.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UsuarioSubeId", dto.UploadedByUserId);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        private static EvidenceDto MapReaderToDto(SqlDataReader reader)
        {
            return new EvidenceDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                IncidentId = reader.GetInt32(reader.GetOrdinal("IncidenciaId")),
                EvidenceType = reader.GetString(reader.GetOrdinal("TipoEvidencia")),
                FilePath = reader.GetString(reader.GetOrdinal("RutaArchivo")),
                Description = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                UploadedByUserId = reader.GetInt32(reader.GetOrdinal("UsuarioSubeId")),
                UploadedByUserName = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                UploadedAt = reader.GetDateTime(reader.GetOrdinal("FechaSubida"))
            };
        }
    }
}
