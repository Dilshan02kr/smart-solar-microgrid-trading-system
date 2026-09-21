namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record ProsumerRegistrationResponse(
    string UserId,
    string Nic,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role,
    string AccountStatus);
