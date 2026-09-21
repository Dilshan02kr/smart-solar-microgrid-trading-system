namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record AuthenticatedUserResponse(
    string UserId,
    string FirstName,
    string LastName,
    string Role,
    string AccountStatus,
    string? Nic = null,
    string? AssignedMicrogridNodeId = null);
