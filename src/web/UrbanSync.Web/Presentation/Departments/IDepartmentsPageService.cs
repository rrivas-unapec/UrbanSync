using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Departments;

public interface IDepartmentsPageService
{
    Task<DepartmentsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        string name,
        int? jurisdictionId,
        CancellationToken cancellationToken = default);
}
