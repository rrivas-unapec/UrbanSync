using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Institution;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories
{
    public sealed class InstitutionRepository : IInstitutionRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public InstitutionRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<InstitutionDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var list = new List<InstitutionDto>();
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT Id, Nombre, TipoInstitucion, ContactoEmail, ContactoTelefono, Activo
            FROM dbo.Instituciones
            WHERE Activo = 1
            ORDER BY Nombre ASC;";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        public async Task<InstitutionDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            SELECT Id, Nombre, TipoInstitucion, ContactoEmail, ContactoTelefono, Activo
            FROM dbo.Instituciones
            WHERE Id = @Id;";

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
            CreateInstitutionDto dto,
            CancellationToken cancellationToken = default)
        {
            using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            const string query = @"
            INSERT INTO dbo.Instituciones (Nombre, TipoInstitucion, ContactoEmail, ContactoTelefono)
            OUTPUT INSERTED.Id
            VALUES (@Nombre, @TipoInstitucion, @ContactoEmail, @ContactoTelefono);";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);
            cmd.Parameters.AddWithValue("@Nombre", dto.Name);
            cmd.Parameters.AddWithValue("@TipoInstitucion", dto.InstitutionType);
            cmd.Parameters.AddWithValue("@ContactoEmail", (object?)dto.ContactEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactoTelefono", (object?)dto.ContactPhone ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }

        private static InstitutionDto MapReaderToDto(SqlDataReader reader)
        {
            return new InstitutionDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Nombre")),
                InstitutionType = reader.GetString(reader.GetOrdinal("TipoInstitucion")),
                ContactEmail = reader.IsDBNull(reader.GetOrdinal("ContactoEmail")) ? null : reader.GetString(reader.GetOrdinal("ContactoEmail")),
                ContactPhone = reader.IsDBNull(reader.GetOrdinal("ContactoTelefono")) ? null : reader.GetString(reader.GetOrdinal("ContactoTelefono")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("Activo"))
            };
        }
    }
}
