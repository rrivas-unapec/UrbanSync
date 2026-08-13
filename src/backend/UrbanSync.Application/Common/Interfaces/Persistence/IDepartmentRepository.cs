using UrbanSync.Application.Features.Departament;

namespace UrbanSync.Application.Common.Interfaces.Persistence
{
    public interface IDepartmentRepository
    {
        Task<IReadOnlyList<DepartmentDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<DepartmentDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateAsync(
            CreateDepartmentDto dto,
            CancellationToken cancellationToken = default);
    }
}
