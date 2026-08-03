using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Domain.Entities;

namespace UrbanSync.Application.Features.Roles;

public class RolService : IRolService
{
    private readonly IRolRepository _rolRepository;

    public RolService(IRolRepository rolRepository)
    {
        _rolRepository = rolRepository;
    }

    public async Task<IEnumerable<RolDto>> GetAllAsync()
    {
        var roles = await _rolRepository.GetAllAsync();

        return roles.Select(rol => new RolDto
        {
            Id = rol.Id,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion
        });
    }

    public async Task<RolDto?> GetByIdAsync(int id)
    {
        var rol = await _rolRepository.GetByIdAsync(id);

        return rol is null
            ? null
            : new RolDto
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion
            };
    }

    public async Task<RolDto> CreateAsync(RolCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            throw new ArgumentException(
                "El nombre del rol es obligatorio.",
                nameof(dto));
        }

        var rol = new Rol
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        var newId = await _rolRepository.CreateAsync(rol);

        return new RolDto
        {
            Id = newId,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };
    }
}