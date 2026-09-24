// Exposes safe Web-user account data without password or authentication material.
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record WebUserResponse(
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role,
    string AccountStatus,
    string? AssignedMicrogridNodeId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
