using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.IncidentType;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories
{
    public sealed class IncidentTypeRepository : IIncidentTypeRepository
    {
        private readonly IDbConnectionFactory _dbconnectionFactory;

        public IncidentTypeRepository(IDbConnectionFactory dbFactory)
        {
            _dbconnectionFactory = dbFactory;
        }

        public async Task<IReadOnlyList<IncidentTypeDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var list = new List<IncidentTypeDto>();
            using var conn = _dbconnectionFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT ti.Id, ti.Nombre, ti.Descripcion, ti.InstitucionId, 
                   inst.Nombre AS NombreInstitucion, ti.Activo
            FROM dbo.TiposIncidencia ti
            INNER JOIN dbo.Instituciones inst ON ti.InstitucionId = inst.Id
            WHERE ti.Activo = 1
            ORDER BY ti.Nombre ASC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<IncidentTypeDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbconnectionFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT ti.Id, ti.Nombre, ti.Descripcion, ti.InstitucionId, 
                   inst.Nombre AS NombreInstitucion, ti.Activo
            FROM dbo.TiposIncidencia ti
            INNER JOIN dbo.Instituciones inst ON ti.InstitucionId = inst.Id
            WHERE ti.Id = @Id;";

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
            CreateIncidentTypeDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbconnectionFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            INSERT INTO dbo.TiposIncidencia (Nombre, Descripcion, InstitucionId)
            OUTPUT INSERTED.Id
            VALUES (@Nombre, @Descripcion, @InstitucionId);";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Nombre", dto.Name);
            cmd.Parameters.AddWithValue("@Descripcion", (object?)dto.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@InstitucionId", dto.InstitutionId);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        private static IncidentTypeDto MapReaderToDto(SqlDataReader reader)
        {
            return new IncidentTypeDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Nombre")),
                Description = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                InstitutionId = reader.GetInt32(reader.GetOrdinal("InstitucionId")),
                InstitutionName = reader.GetString(reader.GetOrdinal("NombreInstitucion")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("Activo"))
            };
        }
    }
}
