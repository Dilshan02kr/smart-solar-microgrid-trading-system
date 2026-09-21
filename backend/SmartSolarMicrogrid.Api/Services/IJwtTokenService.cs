using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Services;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(UserDetails user);
}

public sealed record JwtTokenResult(string Token, DateTime ExpiresAtUtc);
