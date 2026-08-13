using UrbanSync.Web.ApiClients.Claims;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Claims;

public sealed class ClaimsPageService : IClaimsPageService
{
    private readonly IClaimsApiClient _claimsApiClient;
    private readonly ILogger<ClaimsPageService> _logger;

    public ClaimsPageService(
        IClaimsApiClient claimsApiClient,
        ILogger<ClaimsPageService> logger)
    {
        _claimsApiClient = claimsApiClient;
        _logger = logger;
    }

    public async Task<ClaimsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var claims = await _claimsApiClient.GetAllAsync(
                cancellationToken);

            return new ClaimsViewModel
            {
                DatosDisponibles = true,
                Reclamaciones = claims
                    .Select(claim => new ClaimItemViewModel
                    {
                        Id = claim.Id,
                        CitizenUserName = claim.CitizenUserName,
                        LocationAddress = claim.LocationAddress,
                        Category = claim.Category,
                        Title = claim.Title,
                        Description = claim.Description,
                        Status = claim.Status,
                        CreatedAt = claim.CreatedAt
                    })
                    .ToList()
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo construir el listado de reclamaciones.");

            return new ClaimsViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    public Task UpdateStatusAsync(
        int id,
        string status,
        CancellationToken cancellationToken = default)
    {
        return _claimsApiClient.UpdateStatusAsync(
            id,
            new UpdateClaimStatusRequest
            {
                Status = status
            },
            cancellationToken);
    }
}
