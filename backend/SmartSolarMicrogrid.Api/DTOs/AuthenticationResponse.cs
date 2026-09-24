// Returns an issued access token together with expiration and authenticated-user details.
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record AuthenticationResponse(
    string Token,
    DateTime ExpiresAtUtc,
    AuthenticatedUserResponse User);
