using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IProsumerManagementService
{
    Task<IReadOnlyList<ProsumerResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProsumerResponse>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    Task<ProsumerManagementResult> GetByIdAsync(
        string prosumerId,
        CancellationToken cancellationToken = default);

    Task<ProsumerManagementResult> ActivateAsync(
        string prosumerId,
        CancellationToken cancellationToken = default);

    Task<ProsumerManagementResult> ReactivateAsync(
        string prosumerId,
        CancellationToken cancellationToken = default);
}

public enum ProsumerManagementStatus
{
    Success,
    InvalidId,
    NotFound,
    InvalidAccountState
}

public sealed record ProsumerManagementResult(
    ProsumerManagementStatus Status,
    ProsumerResponse? Prosumer = null);
