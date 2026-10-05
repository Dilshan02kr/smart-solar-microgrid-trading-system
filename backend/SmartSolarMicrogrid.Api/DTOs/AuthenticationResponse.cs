/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: AuthenticationResponse.cs
 * Component: Authentication and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Returns an issued access token together with expiration and authenticated-user details.
 */
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record AuthenticationResponse(
    string Token,
    DateTime ExpiresAtUtc,
    AuthenticatedUserResponse User);
