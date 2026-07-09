using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Domain.Entities;

namespace UrbanSync.DataAccess.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UsuarioRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private static Usuario Map(SqlDataReader reader) => new()
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario")),
            NombreCompleto = reader.GetString(reader.GetOrdinal("NombreCompleto")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PasswordHash = (byte[])reader["PasswordHash"],
            PasswordSalt = (byte[])reader["PasswordSalt"],
            RolId = reader.GetInt32(reader.GetOrdinal("RolId")),
            Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
            FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
        };

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            var usuarios = new List<Usuario>();
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                @"SELECT Id, NombreUsuario, NombreCompleto, Email, PasswordHash, PasswordSalt,
                     RolId, Activo, FechaCreacion
              FROM dbo.Usuarios WHERE Activo = 1", connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                usuarios.Add(Map(reader));

            return usuarios;
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                @"SELECT Id, NombreUsuario, NombreCompleto, Email, PasswordHash, PasswordSalt,
                     RolId, Activo, FechaCreacion
              FROM dbo.Usuarios WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                @"SELECT Id, NombreUsuario, NombreCompleto, Email, PasswordHash, PasswordSalt,
                     RolId, Activo, FechaCreacion
              FROM dbo.Usuarios WHERE NombreUsuario = @NombreUsuario", connection);
            command.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<int> CreateAsync(Usuario usuario)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                @"INSERT INTO dbo.Usuarios
                (NombreUsuario, NombreCompleto, Email, PasswordHash, PasswordSalt, RolId, Activo)
              VALUES (@NombreUsuario, @NombreCompleto, @Email, @PasswordHash, @PasswordSalt, @RolId, 1);
              SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);

            command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
            command.Parameters.AddWithValue("@NombreCompleto", usuario.NombreCompleto);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);
            command.Parameters.AddWithValue("@PasswordSalt", usuario.PasswordSalt);
            command.Parameters.AddWithValue("@RolId", usuario.RolId);

            await connection.OpenAsync();
            var newId = await command.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }
    }
}
