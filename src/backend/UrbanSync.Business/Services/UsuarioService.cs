using UrbanSync.Business.Helpers;
using UrbanSync.DataAccess.Repositories;
using UrbanSync.Domain.DTOs;
using UrbanSync.Domain.Entities;

namespace UrbanSync.Business.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRolRepository _rolRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository, IRolRepository rolRepository)
        {
            _usuarioRepository = usuarioRepository;
            _rolRepository = rolRepository;
        }

        private async Task<UsuarioDto> ToDtoAsync(Usuario u)
        {
            var rol = await _rolRepository.GetByIdAsync(u.RolId);
            return new UsuarioDto
            {
                Id = u.Id,
                NombreUsuario = u.NombreUsuario,
                NombreCompleto = u.NombreCompleto,
                Email = u.Email,
                RolId = u.RolId,
                RolNombre = rol?.Nombre ?? string.Empty,
                Activo = u.Activo
            };
        }

        public async Task<IEnumerable<UsuarioDto>> GetAllAsync()
        {
            var usuarios = await _usuarioRepository.GetAllAsync();
            var result = new List<UsuarioDto>();

            foreach (var usuario in usuarios)
                result.Add(await ToDtoAsync(usuario));

            return result;
        }

        public async Task<UsuarioDto?> GetByIdAsync(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            return usuario is null ? null : await ToDtoAsync(usuario);
        }

        public async Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto)
        {
            var existentePorUsuario = await _usuarioRepository.GetByNombreUsuarioAsync(dto.NombreUsuario);
            if (existentePorUsuario is not null)
                throw new ArgumentException("Ya existe un usuario con ese nombre de usuario.");

            var existentePorCorreo = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (existentePorCorreo is not null)
                throw new ArgumentException("Ya existe un usuario con ese correo.");

            var rol = await _rolRepository.GetByIdAsync(dto.RolId);
            if (rol is null)
                throw new ArgumentException("El rol indicado no existe.");

            var (hash, salt) = PasswordHasher.Hash(dto.Password);

            var entity = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                RolId = dto.RolId,
                Activo = true
            };

            var newId = await _usuarioRepository.CreateAsync(entity);
            entity.Id = newId;
            return await ToDtoAsync(entity);
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);

            if (usuario is null || !usuario.Activo)
                return null;

            if (!PasswordHasher.Verify(dto.Password, usuario.PasswordHash, usuario.PasswordSalt))
                return null;

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
    }
}
