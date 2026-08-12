using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Job;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories
{
    public sealed class JobRepository : IJobRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public JobRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<JobDto>> GetAllAsync(
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            var list = new List<JobDto>();
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT t.Id, t.IncidenciaId, i.CodigoCaso, t.UsuarioAsignadoId, u.NombreUsuario,
                   t.DescripcionTrabajo, t.Estado, t.FechaInicio, t.FechaFin, t.Resultado
            FROM dbo.Trabajos t
            INNER JOIN dbo.Usuarios u ON t.UsuarioAsignadoId = u.Id
            INNER JOIN dbo.Incidencias i ON t.IncidenciaId = i.Id
            WHERE (@Estado IS NULL OR t.Estado = @Estado)
            ORDER BY t.Id DESC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Estado", (object?)status ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<IReadOnlyList<JobDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default)
        {
            var list = new List<JobDto>();
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT t.Id, t.IncidenciaId, i.CodigoCaso, t.UsuarioAsignadoId, u.NombreUsuario,
                   t.DescripcionTrabajo, t.Estado, t.FechaInicio, t.FechaFin, t.Resultado
            FROM dbo.Trabajos t
            INNER JOIN dbo.Usuarios u ON t.UsuarioAsignadoId = u.Id
            INNER JOIN dbo.Incidencias i ON t.IncidenciaId = i.Id
            WHERE t.IncidenciaId = @IncidentId
            ORDER BY t.Id DESC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@IncidentId", incidentId);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<JobDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT t.Id, t.IncidenciaId, i.CodigoCaso, t.UsuarioAsignadoId, u.NombreUsuario,
                   t.DescripcionTrabajo, t.Estado, t.FechaInicio, t.FechaFin, t.Resultado
            FROM dbo.Trabajos t
            INNER JOIN dbo.Usuarios u ON t.UsuarioAsignadoId = u.Id
            INNER JOIN dbo.Incidencias i ON t.IncidenciaId = i.Id
            WHERE t.Id = @Id;";

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
            CreateJobDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            INSERT INTO dbo.Trabajos (IncidenciaId, UsuarioAsignadoId, DescripcionTrabajo, Estado, FechaInicio, FechaFin, Resultado)
            OUTPUT INSERTED.Id
            VALUES (@IncidenciaId, @UsuarioAsignadoId, @DescripcionTrabajo, @Estado, @FechaInicio, @FechaFin, @Resultado);";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@IncidenciaId", dto.IncidentId);
            cmd.Parameters.AddWithValue("@UsuarioAsignadoId", dto.AssignedUserId);
            cmd.Parameters.AddWithValue("@DescripcionTrabajo", dto.JobDescription);
            cmd.Parameters.AddWithValue("@Estado", dto.Status);
            cmd.Parameters.AddWithValue("@FechaInicio", (object?)dto.StartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaFin", (object?)dto.EndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Resultado", (object?)dto.Result ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(
            UpdateJobDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            UPDATE dbo.Trabajos
            SET Estado = @Estado,
                FechaInicio = @FechaInicio,
                FechaFin = @FechaFin,
                Resultado = @Resultado
            WHERE Id = @Id;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Id", dto.Id);
            cmd.Parameters.AddWithValue("@Estado", dto.Status);
            cmd.Parameters.AddWithValue("@FechaInicio", (object?)dto.StartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaFin", (object?)dto.EndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Resultado", (object?)dto.Result ?? DBNull.Value);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }   

        private static JobDto MapReaderToDto(SqlDataReader reader)
        {
            return new JobDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                IncidentId = reader.GetInt32(reader.GetOrdinal("IncidenciaId")),
                CodigoCaso = reader.GetString(reader.GetOrdinal("CodigoCaso")),
                AssignedUserId = reader.GetInt32(reader.GetOrdinal("UsuarioAsignadoId")),
                AssignedUserName = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                JobDescription = reader.GetString(reader.GetOrdinal("DescripcionTrabajo")),
                Status = reader.GetString(reader.GetOrdinal("Estado")),
                StartDate = reader.IsDBNull(reader.GetOrdinal("FechaInicio")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaInicio")),
                EndDate = reader.IsDBNull(reader.GetOrdinal("FechaFin")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaFin")),
                Result = reader.IsDBNull(reader.GetOrdinal("Resultado")) ? null : reader.GetString(reader.GetOrdinal("Resultado"))
            };
        }
    }
}
