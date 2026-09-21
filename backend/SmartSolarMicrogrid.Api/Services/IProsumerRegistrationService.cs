using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IProsumerRegistrationService
{
    Task<ProsumerRegistrationResult> RegisterAsync(
        RegisterProsumerRequest request,
        CancellationToken cancellationToken = default);
}

public enum ProsumerRegistrationStatus
{
    Created,
    Invalid,
    NicAlreadyExists,
    EmailAlreadyExists,
    Conflict
}

public sealed record ProsumerRegistrationResult(
    ProsumerRegistrationStatus Status,
    ProsumerRegistrationResponse? Prosumer = null,
    string? Message = null);
