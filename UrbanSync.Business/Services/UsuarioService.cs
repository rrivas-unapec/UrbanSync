
using UrbanSync.Business.Helpers;
using UrbanSync.DataAccess.Repositories;
using UrbanSync.Domain.DTOs;
using UrbanSync.Domain.Entities;

namespace UrbanSync.Business.Services
{

    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        private static UsuarioDto ToDto(Usuario u) => new()
        {
            Id = u.Id,
            NombreUsuario = u.NombreUsuario,
            NombreCompleto = u.NombreCompleto,
            Email = u.Email,
            RolId = u.RolId,
            Activo = u.Activo
        };

        public async Task<IEnumerable<UsuarioDto>> GetAllAsync()
        {
            var usuarios = await _usuarioRepository.GetAllAsync();
            return usuarios.Select(ToDto);
        }

        public async Task<UsuarioDto?> GetByIdAsync(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            return usuario is null ? null : ToDto(usuario);
        }

        public async Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto)
        {
            var existente = await _usuarioRepository.GetByNombreUsuarioAsync(dto.NombreUsuario);
            if (existente is not null)
                throw new ArgumentException("Ya existe un usuario con ese nombre de usuario.");

            var (hash, salt) = PasswordHasher.Hash(dto.Password);

            var entity = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                RolId = dto.RolId
            };

            var newId = await _usuarioRepository.CreateAsync(entity);
            entity.Id = newId;
            return ToDto(entity);
        }
    }
}
