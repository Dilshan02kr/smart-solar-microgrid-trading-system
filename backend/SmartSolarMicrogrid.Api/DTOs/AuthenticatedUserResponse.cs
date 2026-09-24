// Returns the authenticated user's safe identity, role, status, and applicable assignment data.
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record AuthenticatedUserResponse(
    string UserId,
    string FirstName,
    string LastName,
    string Role,
    string AccountStatus,
    string? Nic = null,
    string? AssignedMicrogridNodeId = null);
