using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.TechnicalAnalysis;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories
{
    public sealed class TechnicalAnalysisRepository : ITechnicalAnalysisRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public TechnicalAnalysisRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<TechnicalAnalysisDto?> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT a.Id, a.IncidenciaId, a.UsuarioTecnicoId, u.NombreUsuario, 
                   a.Diagnostico, a.AccionesRecomendadas, a.FechaAnalisis
            FROM dbo.AnalisisTecnico a
            INNER JOIN dbo.Usuarios u ON a.UsuarioTecnicoId = u.Id
            WHERE a.IncidenciaId = @IncidentId;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@IncidentId", incidentId);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapReaderToDto(reader);
            }

            return null;
        }

        public async Task<TechnicalAnalysisDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT a.Id, a.IncidenciaId, a.UsuarioTecnicoId, u.NombreUsuario, 
                   a.Diagnostico, a.AccionesRecomendadas, a.FechaAnalisis
            FROM dbo.AnalisisTecnico a
            INNER JOIN dbo.Usuarios u ON a.UsuarioTecnicoId = u.Id
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

        public async Task<int> CreateAsync(
            CreateTechnicalAnalysisDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            INSERT INTO dbo.AnalisisTecnico (IncidenciaId, UsuarioTecnicoId, Diagnostico, AccionesRecomendadas)
            OUTPUT INSERTED.Id
            VALUES (@IncidenciaId, @UsuarioTecnicoId, @Diagnostico, @AccionesRecomendadas);";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@IncidenciaId", dto.IncidentId);
            cmd.Parameters.AddWithValue("@UsuarioTecnicoId", dto.TechnicalUserId);
            cmd.Parameters.AddWithValue("@Diagnostico", dto.Diagnosis);
            cmd.Parameters.AddWithValue("@AccionesRecomendadas", (object?)dto.RecommendedActions ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        private static TechnicalAnalysisDto MapReaderToDto(SqlDataReader reader)
        {
            return new TechnicalAnalysisDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                IncidentId = reader.GetInt32(reader.GetOrdinal("IncidenciaId")),
                TechnicalUserId = reader.GetInt32(reader.GetOrdinal("UsuarioTecnicoId")),
                TechnicalUserName = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                Diagnosis = reader.GetString(reader.GetOrdinal("Diagnostico")),
                RecommendedActions = reader.IsDBNull(reader.GetOrdinal("AccionesRecomendadas")) ? null : reader.GetString(reader.GetOrdinal("AccionesRecomendadas")),
                AnalysisDate = reader.GetDateTime(reader.GetOrdinal("FechaAnalisis"))
            };
        }
    }
}
