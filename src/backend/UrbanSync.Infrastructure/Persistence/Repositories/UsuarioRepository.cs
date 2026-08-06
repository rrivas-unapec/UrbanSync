using System.Data;
using Microsoft.Data.SqlClient;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Domain.Entities;
using UrbanSync.Infrastructure.Persistence.Connections;

namespace UrbanSync.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private const string SelectUsuario =
        """
        SELECT
            Id,
            NombreUsuario,
            NombreCompleto,
            Email,
            PasswordHash,
            PasswordSalt,
            RolId,
            Activo,
            FechaCreacion
        FROM dbo.Usuarios
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public UsuarioRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        var usuarios = new List<Usuario>();

        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            $"{SelectUsuario} ORDER BY Id DESC",
            connection);

        await connection.OpenAsync();

        using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            usuarios.Add(Map(reader));
        }

        return usuarios;
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            $"{SelectUsuario} WHERE Id = @Id",
            connection);

        command.Parameters
            .Add("@Id", SqlDbType.Int)
            .Value = id;

        await connection.OpenAsync();

        using var reader =
            await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? Map(reader)
            : null;
    }

    public async Task<Usuario?> GetByNombreUsuarioAsync(
        string nombreUsuario)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            $"{SelectUsuario} WHERE NombreUsuario = @NombreUsuario",
            connection);

        command.Parameters
            .Add(
                "@NombreUsuario",
                SqlDbType.NVarChar,
                100)
            .Value = nombreUsuario;

        await connection.OpenAsync();

        using var reader =
            await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? Map(reader)
            : null;
    }

    public async Task<Usuario?> GetByEmailAsync(
        string email)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            $"{SelectUsuario} WHERE Email = @Email",
            connection);

        command.Parameters
            .Add(
                "@Email",
                SqlDbType.NVarChar,
                150)
            .Value = email;

        await connection.OpenAsync();

        using var reader =
            await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? Map(reader)
            : null;
    }

    public async Task<int> CreateAsync(
        Usuario usuario)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            """
            INSERT INTO dbo.Usuarios
            (
                NombreUsuario,
                NombreCompleto,
                Email,
                PasswordHash,
                PasswordSalt,
                RolId,
                Activo
            )
            VALUES
            (
                @NombreUsuario,
                @NombreCompleto,
                @Email,
                @PasswordHash,
                @PasswordSalt,
                @RolId,
                1
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """,
            connection);

        command.Parameters
            .Add(
                "@NombreUsuario",
                SqlDbType.NVarChar,
                100)
            .Value = usuario.NombreUsuario;

        command.Parameters
            .Add(
                "@NombreCompleto",
                SqlDbType.NVarChar,
                150)
            .Value = usuario.NombreCompleto;

        command.Parameters
            .Add(
                "@Email",
                SqlDbType.NVarChar,
                150)
            .Value = usuario.Email;

        command.Parameters
            .Add(
                "@PasswordHash",
                SqlDbType.VarBinary,
                usuario.PasswordHash.Length)
            .Value = usuario.PasswordHash;

        command.Parameters
            .Add(
                "@PasswordSalt",
                SqlDbType.VarBinary,
                usuario.PasswordSalt.Length)
            .Value = usuario.PasswordSalt;

        command.Parameters
            .Add("@RolId", SqlDbType.Int)
            .Value = usuario.RolId;

        await connection.OpenAsync();

        var newId =
            await command.ExecuteScalarAsync();

        return Convert.ToInt32(newId);
    }

    public async Task<bool> ToggleStatusAsync(
        int id)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            """
            UPDATE dbo.Usuarios
            SET
                Activo = CASE
                    WHEN Activo = 1 THEN 0
                    ELSE 1
                END,
                FechaModificacion = SYSDATETIME()
            WHERE Id = @Id;
            """,
            connection);

        command.Parameters
            .Add("@Id", SqlDbType.Int)
            .Value = id;

        await connection.OpenAsync();

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> UpdatePasswordAsync(
        int id,
        byte[] passwordHash,
        byte[] passwordSalt)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var command = new SqlCommand(
            """
            UPDATE dbo.Usuarios
            SET
                PasswordHash = @PasswordHash,
                PasswordSalt = @PasswordSalt,
                FechaModificacion = SYSDATETIME()
            WHERE Id = @Id
              AND Activo = 1;
            """,
            connection);

        command.Parameters
            .Add(
                "@PasswordHash",
                SqlDbType.VarBinary,
                passwordHash.Length)
            .Value = passwordHash;

        command.Parameters
            .Add(
                "@PasswordSalt",
                SqlDbType.VarBinary,
                passwordSalt.Length)
            .Value = passwordSalt;

        command.Parameters
            .Add("@Id", SqlDbType.Int)
            .Value = id;

        await connection.OpenAsync();

        var affectedRows =
            await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    private static Usuario Map(
        SqlDataReader reader)
    {
        return new Usuario
        {
            Id = reader.GetInt32(
                reader.GetOrdinal("Id")),

            NombreUsuario = reader.GetString(
                reader.GetOrdinal("NombreUsuario")),

            NombreCompleto = reader.GetString(
                reader.GetOrdinal("NombreCompleto")),

            Email = reader.GetString(
                reader.GetOrdinal("Email")),

            PasswordHash =
                (byte[])reader["PasswordHash"],

            PasswordSalt =
                (byte[])reader["PasswordSalt"],

            RolId = reader.GetInt32(
                reader.GetOrdinal("RolId")),

            Activo = reader.GetBoolean(
                reader.GetOrdinal("Activo")),

            FechaCreacion = reader.GetDateTime(
                reader.GetOrdinal("FechaCreacion"))
        };
    }
}