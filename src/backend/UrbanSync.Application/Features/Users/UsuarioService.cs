using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Authentication;
using UrbanSync.Application.Common.Interfaces.Authentication;
using UrbanSync.Domain.Entities;

namespace UrbanSync.Application.Features.Users;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        var result = new List<UsuarioDto>();

        foreach (var usuario in usuarios)
        {
            result.Add(await ToDtoAsync(usuario));
        }

        return result;
    }

    public async Task<UsuarioDto?> GetByIdAsync(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);

        return usuario is null
            ? null
            : await ToDtoAsync(usuario);
    }

    public async Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto)
    {
        var existingUser = await _usuarioRepository
            .GetByNombreUsuarioAsync(dto.NombreUsuario);

        if (existingUser is not null)
        {
            throw new ArgumentException(
                "Ya existe un usuario con ese nombre de usuario.",
                nameof(dto));
        }

        var existingEmail = await _usuarioRepository
            .GetByEmailAsync(dto.Email);

        if (existingEmail is not null)
        {
            throw new ArgumentException(
                "Ya existe un usuario con ese correo.",
                nameof(dto));
        }

        var rol = await _rolRepository.GetByIdAsync(dto.RolId);

        if (rol is null)
        {
            throw new ArgumentException(
                "El rol indicado no existe.",
                nameof(dto));
        }

        var (hash, salt) = _passwordHasher.Hash(dto.Password);

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

        usuario.Id = await _usuarioRepository.CreateAsync(usuario);

        return await ToDtoAsync(usuario);
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);

        if (usuario is null || !usuario.Activo)
        {
            return null;
        }

        var isValidPassword = _passwordHasher.Verify(
            dto.Password,
            usuario.PasswordHash,
            usuario.PasswordSalt);

        if (!isValidPassword)
        {
            return null;
        }

        return new LoginResponseDto
        {
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            User = await ToDtoAsync(usuario)
        };
    }

    public Task<bool> ToggleStatusAsync(int id)
    {
        return _usuarioRepository.ToggleStatusAsync(id);
    }

    private async Task<UsuarioDto> ToDtoAsync(Usuario usuario)
    {
        var rol = await _rolRepository.GetByIdAsync(usuario.RolId);

        return new UsuarioDto
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Email = usuario.Email,
            RolId = usuario.RolId,
            RolNombre = rol?.Nombre ?? string.Empty,
            Activo = usuario.Activo
        };
    }
}