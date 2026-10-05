/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IJwtTokenService.cs
 * Component: Authentication and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines creation of signed access tokens from authoritative user records.
 */
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Services;

public interface IJwtTokenService
{
    // Creates a signed time-limited token for an authoritative user record.
    JwtTokenResult CreateToken(UserDetails user);
}

public sealed record JwtTokenResult(string Token, DateTime ExpiresAtUtc);
