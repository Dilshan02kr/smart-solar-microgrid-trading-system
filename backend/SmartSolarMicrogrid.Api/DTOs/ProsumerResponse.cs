namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record ProsumerResponse(
    string UserId,
    string Nic,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string AccountStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt);
