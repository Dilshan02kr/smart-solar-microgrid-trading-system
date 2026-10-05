/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IStationManagementService.cs
 * Component: Microgrid Node and Station Management
 * Component Owner: R A K Hansika (IT23140998)
 *
 * Purpose:
 * Defines validated station creation, update, activation, and deactivation operations.
 */
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IStationManagementService
{
    // Returns all stations as public response DTOs.
    Task<IReadOnlyList<StationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // Validates and retrieves one station.
    Task<StationManagementResult> GetByIdAsync(
        string stationId,
        CancellationToken cancellationToken = default);

    // Validates and creates an active station.
    Task<StationManagementResult> CreateAsync(
        CreateStationRequest request,
        CancellationToken cancellationToken = default);

    // Validates and updates mutable station details.
    Task<StationManagementResult> UpdateAsync(
        string stationId,
        UpdateStationRequest request,
        CancellationToken cancellationToken = default);

    // Transitions an inactive station to active.
    Task<StationManagementResult> ActivateAsync(
        string stationId,
        CancellationToken cancellationToken = default);

    // Deactivates an active station when reservations do not block it.
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
    InvalidState,
    HasActiveReservations
}

public sealed record StationManagementResult(
    StationManagementStatus Status,
    StationResponse? Station = null,
    string? ErrorMessage = null);
