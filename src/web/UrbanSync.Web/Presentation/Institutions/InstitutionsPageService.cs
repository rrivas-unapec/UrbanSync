using UrbanSync.Web.ApiClients.Institutions;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Institutions;

public sealed class InstitutionsPageService : IInstitutionsPageService
{
    private readonly IInstitutionsApiClient _institutionsApiClient;
    private readonly ILogger<InstitutionsPageService> _logger;

    public InstitutionsPageService(
        IInstitutionsApiClient institutionsApiClient,
        ILogger<InstitutionsPageService> logger)
    {
        _institutionsApiClient = institutionsApiClient;
        _logger = logger;
    }

    public async Task<InstitutionsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var institutions = await _institutionsApiClient.GetAllAsync(
                cancellationToken);

            return new InstitutionsViewModel
            {
                DatosDisponibles = true,
                Instituciones = institutions
                    .Select(institution => new InstitutionItemViewModel
                    {
                        Id = institution.Id,
                        Name = institution.Name,
                        InstitutionType = institution.InstitutionType,
                        ContactEmail = institution.ContactEmail,
                        ContactPhone = institution.ContactPhone,
                        IsActive = institution.IsActive
                    })
                    .ToList()
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo construir el listado de instituciones.");

            return new InstitutionsViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    public Task CreateAsync(
        string name,
        string institutionType,
        string? contactEmail,
        string? contactPhone,
        CancellationToken cancellationToken = default)
    {
        return _institutionsApiClient.CreateAsync(
            new CreateInstitutionRequest
            {
                Name = name,
                InstitutionType = institutionType,
                ContactEmail = contactEmail,
                ContactPhone = contactPhone
            },
            cancellationToken);
    }
}
