using Microsoft.Data.SqlClient;
using UrbanSync.Domain.Entities;
using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Infrastructure.Repositories
{
    public class RolRepository : IRolRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public RolRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Rol>> GetAllAsync()
        {
            var roles = new List<Rol>();
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                "SELECT Id, Nombre, Descripcion, Activo, FechaCreacion FROM dbo.Roles WHERE Activo = 1",
                connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                roles.Add(new Rol
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                    Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                    FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
                });
            }
            return roles;
        }

        public async Task<Rol?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                "SELECT Id, Nombre, Descripcion, Activo, FechaCreacion FROM dbo.Roles WHERE Id = @Id",
                connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Rol
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                    Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                    FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
                };
            }
            return null;
        }

        public async Task<int> CreateAsync(Rol rol)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                @"INSERT INTO dbo.Roles (Nombre, Descripcion, Activo)
              VALUES (@Nombre, @Descripcion, 1);
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
                connection);

            command.Parameters.AddWithValue("@Nombre", rol.Nombre);
            command.Parameters.AddWithValue("@Descripcion", (object?)rol.Descripcion ?? DBNull.Value);

            await connection.OpenAsync();
            var newId = await command.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }
    }
}
