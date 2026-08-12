
namespace UrbanSync.Application.Features.Departament
{
    public interface IDepartmentService
    {
        Task<IReadOnlyList<DepartmentDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<DepartmentDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<DepartmentDto> CreateAsync(
            CreateDepartmentDto dto,
            CancellationToken cancellationToken = default);
    }
}
