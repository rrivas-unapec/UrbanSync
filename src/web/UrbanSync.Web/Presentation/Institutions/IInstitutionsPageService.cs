using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Institutions;

public interface IInstitutionsPageService
{
    Task<InstitutionsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        string name,
        string institutionType,
        string? contactEmail,
        string? contactPhone,
        CancellationToken cancellationToken = default);
}
