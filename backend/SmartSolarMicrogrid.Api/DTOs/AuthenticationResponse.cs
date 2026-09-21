namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record AuthenticationResponse(
    string Token,
    DateTime ExpiresAtUtc,
    AuthenticatedUserResponse User);
