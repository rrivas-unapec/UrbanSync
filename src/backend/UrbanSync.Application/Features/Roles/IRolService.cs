namespace UrbanSync.Application.Features.Roles;

public interface IRolService
{
    Task<IEnumerable<RolDto>> GetAllAsync();

    Task<RolDto?> GetByIdAsync(int id);

    Task<RolDto> CreateAsync(RolCreateDto dto);
}