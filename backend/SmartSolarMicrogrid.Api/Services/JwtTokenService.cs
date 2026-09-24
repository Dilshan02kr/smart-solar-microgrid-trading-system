// Creates signed JWTs containing the authoritative user identity and role claims.
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartSolarMicrogrid.Api.Configuration;
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class JwtTokenService(IOptions<JwtSettings> jwtOptions) : IJwtTokenService
{
    public JwtTokenResult CreateToken(UserDetails user)
    {
        // Creates a time-limited signed JWT containing only user identity and role claims.
        var settings = jwtOptions.Value;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims:
            [
                new Claim("userId", user.Id.ToString()),
                new Claim("role", user.Role.ToString())
            ],
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc);
    }
}
