using Moq;
using UrbanSync.Application.Common.Interfaces.Authentication;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Authentication;
using UrbanSync.Application.Features.Users;
using UrbanSync.Domain.Entities;

namespace UrbanSync.Application.UnitTests.Features.Users;

public sealed class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository>
        _usuarioRepositoryMock;

    private readonly Mock<IRolRepository>
        _rolRepositoryMock;

    private readonly Mock<IPasswordHasher>
        _passwordHasherMock;

    private readonly Mock<ITokenGenerator>
        _tokenGeneratorMock;

    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _usuarioRepositoryMock =
            new Mock<IUsuarioRepository>();

        _rolRepositoryMock =
            new Mock<IRolRepository>();

        _passwordHasherMock =
            new Mock<IPasswordHasher>();

        _tokenGeneratorMock =
            new Mock<ITokenGenerator>();

        _service = new UsuarioService(
            _usuarioRepositoryMock.Object,
            _rolRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenGeneratorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateUser_WhenInputIsValid()
    {
        var request = new UsuarioCreateDto
        {
            NombreUsuario = "carlos",
            NombreCompleto = "Carlos Rodríguez",
            Email = "carlos@urbansync.com",
            Password = "Password123!",
            RolId = 1
        };

        var role = new Rol
        {
            Id = 1,
            Nombre = "Administrador",
            Activo = true
        };

        var hash = new byte[]
        {
            1,
            2,
            3
        };

        var salt = new byte[]
        {
            4,
            5,
            6
        };

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.GetByNombreUsuarioAsync(
                    request.NombreUsuario))
            .ReturnsAsync((Usuario?)null);

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    request.Email))
            .ReturnsAsync((Usuario?)null);

        _rolRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    request.RolId))
            .ReturnsAsync(role);

        _passwordHasherMock
            .Setup(hasher =>
                hasher.Hash(request.Password))
            .Returns((hash, salt));

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.CreateAsync(
                    It.IsAny<Usuario>()))
            .ReturnsAsync(10);

        var result = await _service.CreateAsync(request);

        Assert.Equal(10, result.Id);
        Assert.Equal(
            request.NombreUsuario,
            result.NombreUsuario);
        Assert.Equal(
            request.NombreCompleto,
            result.NombreCompleto);
        Assert.Equal(
            request.Email,
            result.Email);
        Assert.Equal(
            request.RolId,
            result.RolId);
        Assert.Equal(
            role.Nombre,
            result.RolNombre);
        Assert.True(result.Activo);

        _usuarioRepositoryMock.Verify(
            repository =>
                repository.CreateAsync(
                    It.Is<Usuario>(usuario =>
                        usuario.NombreUsuario ==
                            request.NombreUsuario &&
                        usuario.NombreCompleto ==
                            request.NombreCompleto &&
                        usuario.Email ==
                            request.Email &&
                        usuario.PasswordHash ==
                            hash &&
                        usuario.PasswordSalt ==
                            salt &&
                        usuario.RolId ==
                            request.RolId &&
                        usuario.Activo)),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUsernameAlreadyExists()
    {
        var request = CreateValidRequest();

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.GetByNombreUsuarioAsync(
                    request.NombreUsuario))
            .ReturnsAsync(new Usuario
            {
                Id = 1,
                NombreUsuario =
                    request.NombreUsuario
            });

        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(request));

        Assert.Contains(
            "nombre de usuario",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        _usuarioRepositoryMock.Verify(
            repository =>
                repository.CreateAsync(
                    It.IsAny<Usuario>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var request = CreateValidRequest();

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.GetByNombreUsuarioAsync(
                    request.NombreUsuario))
            .ReturnsAsync((Usuario?)null);

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    request.Email))
            .ReturnsAsync(new Usuario
            {
                Id = 1,
                Email = request.Email
            });

        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(request));

        Assert.Contains(
            "correo",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        _usuarioRepositoryMock.Verify(
            repository =>
                repository.CreateAsync(
                    It.IsAny<Usuario>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenRoleDoesNotExist()
    {
        var request = CreateValidRequest();

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.GetByNombreUsuarioAsync(
                    request.NombreUsuario))
            .ReturnsAsync((Usuario?)null);

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    request.Email))
            .ReturnsAsync((Usuario?)null);

        _rolRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    request.RolId))
            .ReturnsAsync((Rol?)null);

        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(request));

        Assert.Contains(
            "rol",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        _usuarioRepositoryMock.Verify(
            repository =>
                repository.CreateAsync(
                    It.IsAny<Usuario>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var request = new LoginRequestDto
        {
            Email = "not-found@urbansync.com",
            Password = "Password123!"
        };

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    request.Email))
            .ReturnsAsync((Usuario?)null);

        var result = await _service.LoginAsync(request);

        Assert.Null(result);

        _passwordHasherMock.Verify(
            hasher =>
                hasher.Verify(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<byte[]>()),
            Times.Never);

        _tokenGeneratorMock.Verify(
            generator =>
                generator.Generate(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserIsInactive()
    {
        var request = new LoginRequestDto
        {
            Email = "inactive@urbansync.com",
            Password = "Password123!"
        };

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    request.Email))
            .ReturnsAsync(new Usuario
            {
                Id = 1,
                Email = request.Email,
                Activo = false
            });

        var result = await _service.LoginAsync(request);

        Assert.Null(result);

        _passwordHasherMock.Verify(
            hasher =>
                hasher.Verify(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<byte[]>()),
            Times.Never);

        _tokenGeneratorMock.Verify(
            generator =>
                generator.Generate(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsInvalid()
    {
        var request = new LoginRequestDto
        {
            Email = "user@urbansync.com",
            Password = "WrongPassword"
        };

        var user = CreateActiveUser();

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher =>
                hasher.Verify(
                    request.Password,
                    user.PasswordHash,
                    user.PasswordSalt))
            .Returns(false);

        var result = await _service.LoginAsync(request);

        Assert.Null(result);

        _tokenGeneratorMock.Verify(
            generator =>
                generator.Generate(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnJwt_WhenCredentialsAreValid()
    {
        var request = new LoginRequestDto
        {
            Email = "user@urbansync.com",
            Password = "Password123!"
        };

        var user = CreateActiveUser();

        var role = new Rol
        {
            Id = user.RolId,
            Nombre = "Ciudadano",
            Activo = true
        };

        var expiresAtUtc =
            DateTimeOffset.UtcNow.AddHours(1);

        _usuarioRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher =>
                hasher.Verify(
                    request.Password,
                    user.PasswordHash,
                    user.PasswordSalt))
            .Returns(true);

        _rolRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    user.RolId))
            .ReturnsAsync(role);

        _tokenGeneratorMock
            .Setup(generator =>
                generator.Generate(
                    user.Id,
                    user.NombreCompleto,
                    user.Email,
                    role.Nombre))
            .Returns(new GeneratedToken(
                "header.payload.signature",
                expiresAtUtc));

        var result = await _service.LoginAsync(request);

        Assert.NotNull(result);

        Assert.Equal(
            "header.payload.signature",
            result.Token);

        Assert.Equal(
            expiresAtUtc,
            result.ExpiresAtUtc);

        Assert.Equal(
            user.Id,
            result.User.Id);

        Assert.Equal(
            user.NombreUsuario,
            result.User.NombreUsuario);

        Assert.Equal(
            user.Email,
            result.User.Email);

        Assert.Equal(
            role.Nombre,
            result.User.RolNombre);

        _tokenGeneratorMock.Verify(
            generator =>
                generator.Generate(
                    user.Id,
                    user.NombreCompleto,
                    user.Email,
                    role.Nombre),
            Times.Once);
    }

    private static UsuarioCreateDto CreateValidRequest()
    {
        return new UsuarioCreateDto
        {
            NombreUsuario = "carlos",
            NombreCompleto = "Carlos Rodríguez",
            Email = "carlos@urbansync.com",
            Password = "Password123!",
            RolId = 1
        };
    }

    private static Usuario CreateActiveUser()
    {
        return new Usuario
        {
            Id = 5,
            NombreUsuario = "ciudadano",
            NombreCompleto =
                "Ciudadano UrbanSync",
            Email = "user@urbansync.com",
            PasswordHash =
            [
                1,
                2,
                3
            ],
            PasswordSalt =
            [
                4,
                5,
                6
            ],
            RolId = 6,
            Activo = true
        };
    }
}