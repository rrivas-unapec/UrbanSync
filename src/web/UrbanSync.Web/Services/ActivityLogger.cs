namespace UrbanSync.Web.Services;

public sealed class ActivityLogger
{
    private readonly ILogger<ActivityLogger> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ActivityLogger(ILogger<ActivityLogger> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task LogAsync(string action, string description)
    {
        var user = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonimo";
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        _logger.LogInformation("{Action} | {Description} | User={User} | IP={Ip}", action, description, user, ip);
        return Task.CompletedTask;
    }
}
