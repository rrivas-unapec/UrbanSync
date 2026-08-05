using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UrbanSync.Application.Common.Interfaces.Authentication;

namespace UrbanSync.Infrastructure.Authentication;

public sealed class JwtTokenGenerator : ITokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public GeneratedToken Generate(
        int userId,
        string fullName,
        string email,
        string role)
    {
        var issuedAtUtc = DateTimeOffset.UtcNow;

        var expiresAtUtc = issuedAtUtc.AddMinutes(
            _options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                userId.ToString()),

            new(
                ClaimTypes.NameIdentifier,
                userId.ToString()),

            new(
                ClaimTypes.Name,
                fullName),

            new(
                ClaimTypes.Email,
                email),

            new(
                ClaimTypes.Role,
                role),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString("N"))
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SecretKey));

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAtUtc.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: signingCredentials);

        var accessToken =
            new JwtSecurityTokenHandler().WriteToken(token);

        return new GeneratedToken(
            accessToken,
            expiresAtUtc);
    }
}