using UrbanSync.Application.Common.Interfaces.Authentication;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Authentication;
using UrbanSync.Domain.Entities;

namespace UrbanSync.Application.Features.Users;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllAsync()
    {
        var usuarios =
            await _usuarioRepository.GetAllAsync();

        var result = new List<UsuarioDto>();

        foreach (var usuario in usuarios)
        {
            result.Add(
                await ToDtoAsync(usuario));
        }

        return result;
    }

    public async Task<UsuarioDto?> GetByIdAsync(
        int id)
    {
        var usuario =
            await _usuarioRepository.GetByIdAsync(id);

        return usuario is null
            ? null
            : await ToDtoAsync(usuario);
    }

    public async Task<UsuarioDto> CreateAsync(
        UsuarioCreateDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var existingUser =
            await _usuarioRepository
                .GetByNombreUsuarioAsync(
                    dto.NombreUsuario);

        if (existingUser is not null)
        {
            throw new ArgumentException(
                "Ya existe un usuario con ese nombre de usuario.",
                nameof(dto));
        }

        var existingEmail =
            await _usuarioRepository
                .GetByEmailAsync(dto.Email);

        if (existingEmail is not null)
        {
            throw new ArgumentException(
                "Ya existe un usuario con ese correo.",
                nameof(dto));
        }

        var rol =
            await _rolRepository.GetByIdAsync(
                dto.RolId);

        if (rol is null)
        {
            throw new ArgumentException(
                "El rol indicado no existe.",
                nameof(dto));
        }

        var (hash, salt) =
            _passwordHasher.Hash(dto.Password);

        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario,
            NombreCompleto = dto.NombreCompleto,
            Email = dto.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            RolId = dto.RolId,
            Activo = true
        };

        usuario.Id =
            await _usuarioRepository.CreateAsync(
                usuario);

        return await ToDtoAsync(usuario);
    }

    public async Task<LoginResponseDto?> LoginAsync(
        LoginRequestDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var usuario =
            await _usuarioRepository.GetByEmailAsync(
                dto.Email);

        if (usuario is null || !usuario.Activo)
        {
            return null;
        }

        var isValidPassword =
            _passwordHasher.Verify(
                dto.Password,
                usuario.PasswordHash,
                usuario.PasswordSalt);

        if (!isValidPassword)
        {
            return null;
        }

        var userDto =
            await ToDtoAsync(usuario);

        var generatedToken =
            _tokenGenerator.Generate(
                usuario.Id,
                usuario.NombreCompleto,
                usuario.Email,
                userDto.RolNombre);

        return new LoginResponseDto
        {
            Token = generatedToken.AccessToken,
            ExpiresAtUtc =
                generatedToken.ExpiresAtUtc,
            User = userDto
        };
    }

    public Task<bool> ToggleStatusAsync(
        int id)
    {
        return _usuarioRepository
            .ToggleStatusAsync(id);
    }

    public async Task ChangePasswordAsync(
        ChangePasswordDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.UserId <= 0)
        {
            throw new ArgumentException(
                "El identificador del usuario no es válido.",
                nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(
                dto.CurrentPassword))
        {
            throw new ArgumentException(
                "La contraseña actual es obligatoria.",
                nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(
                dto.NewPassword))
        {
            throw new ArgumentException(
                "La nueva contraseña es obligatoria.",
                nameof(dto));
        }

        if (dto.NewPassword.Length < 8)
        {
            throw new ArgumentException(
                "La nueva contraseña debe tener al menos 8 caracteres.",
                nameof(dto));
        }

        if (string.Equals(
                dto.CurrentPassword,
                dto.NewPassword,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "La nueva contraseña debe ser diferente de la contraseña actual.",
                nameof(dto));
        }

        var usuario =
            await _usuarioRepository.GetByIdAsync(
                dto.UserId);

        if (usuario is null || !usuario.Activo)
        {
            throw new KeyNotFoundException(
                "El usuario no existe o se encuentra inactivo.");
        }

        var currentPasswordIsValid =
            _passwordHasher.Verify(
                dto.CurrentPassword,
                usuario.PasswordHash,
                usuario.PasswordSalt);

        if (!currentPasswordIsValid)
        {
            throw new UnauthorizedAccessException(
                "La contraseña actual es incorrecta.");
        }

        var (newHash, newSalt) =
            _passwordHasher.Hash(
                dto.NewPassword);

        var updated =
            await _usuarioRepository
                .UpdatePasswordAsync(
                    usuario.Id,
                    newHash,
                    newSalt);

        if (!updated)
        {
            throw new InvalidOperationException(
                "No fue posible actualizar la contraseña.");
        }
    }

    private async Task<UsuarioDto> ToDtoAsync(
        Usuario usuario)
    {
        var rol =
            await _rolRepository.GetByIdAsync(
                usuario.RolId);

        return new UsuarioDto
        {
            Id = usuario.Id,
            NombreUsuario =
                usuario.NombreUsuario,
            NombreCompleto =
                usuario.NombreCompleto,
            Email = usuario.Email,
            RolId = usuario.RolId,
            RolNombre =
                rol?.Nombre ?? string.Empty,
            Activo = usuario.Activo
        };
    }
}