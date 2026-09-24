using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IStationManagementService
{
    Task<IReadOnlyList<StationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<StationManagementResult> GetByIdAsync(
        string stationId,
        CancellationToken cancellationToken = default);

    Task<StationManagementResult> CreateAsync(
        CreateStationRequest request,
        CancellationToken cancellationToken = default);

    Task<StationManagementResult> UpdateAsync(
        string stationId,
        UpdateStationRequest request,
        CancellationToken cancellationToken = default);

    Task<StationManagementResult> ActivateAsync(
        string stationId,
        CancellationToken cancellationToken = default);

    Task<StationManagementResult> DeactivateAsync(
        string stationId,
        CancellationToken cancellationToken = default);
}

public enum StationManagementStatus
{
    Success,
    InvalidId,
    NotFound,
    ValidationError,
    DeactivationUnavailable,
    HasActiveReservations
}

public sealed record StationManagementResult(
    StationManagementStatus Status,
    StationResponse? Station = null,
    string? ErrorMessage = null);
