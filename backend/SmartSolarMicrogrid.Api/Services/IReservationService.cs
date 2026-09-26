// Defines reservation workflows and their structured business outcomes.
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IReservationService
{
    // Create a pending reservation owned by the authenticated Prosumer.
    Task<ReservationOperationResult> CreateAsync(
        string userId,
        CreateReservationRequest request,
        CancellationToken cancellationToken = default);

    // Return reservations owned by the authenticated Prosumer.
    Task<ReservationOperationResult> GetMineAsync(
        string userId,
        CancellationToken cancellationToken = default);

    // Return reservations for a validated Prosumer identifier to Backoffice.
    Task<ReservationOperationResult> GetByProsumerAsync(
        string prosumerId,
        CancellationToken cancellationToken = default);

    // Return reservation details to Backoffice or the owning Prosumer.
    Task<ReservationOperationResult> GetByIdAsync(
        string reservationId,
        string userId,
        bool isBackoffice,
        CancellationToken cancellationToken = default);

    // Search reservations using bounded administrative filters.
    Task<ReservationOperationResult> SearchAsync(
        string? prosumerId,
        string? status,
        string? searchTerm,
        CancellationToken cancellationToken = default);

    // Approve one future pending reservation as Backoffice.
    Task<ReservationOperationResult> ApproveAsync(
        string reservationId,
        CancellationToken cancellationToken = default);

    // Replace an owned reservation's station and slot before the cutoff.
    Task<ReservationOperationResult> UpdateAsync(
        string reservationId,
        string userId,
        UpdateReservationRequest request,
        CancellationToken cancellationToken = default);

    // Cancel an owned pending or approved reservation before the cutoff.
    Task<ReservationOperationResult> CancelAsync(
        string reservationId,
        string userId,
        CancellationToken cancellationToken = default);
}

public enum ReservationOperationStatus
{
    Success,
    InvalidUserId,
    InvalidReservationId,
    InvalidProsumerId,
    InvalidStationId,
    InvalidSlotId,
    InvalidReservationStatus,
    ReservationNotFound,
    StationNotFound,
    SlotNotFound,
    StationInactive,
    SlotNotAvailable,
    SlotStationMismatch,
    OutsideAllowedWindow,
    ChangeTooLate,
    InvalidReservationState,
    AccessDenied
}

public sealed record ReservationOperationResult(
    ReservationOperationStatus Status,
    ReservationResponse? Reservation = null,
    IReadOnlyList<ReservationResponse>? Reservations = null);
