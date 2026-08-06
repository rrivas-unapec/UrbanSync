namespace UrbanSync.Web.ApiClients.Authentication;

public sealed class RegisterRequest
{
    public string NombreCompleto { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}