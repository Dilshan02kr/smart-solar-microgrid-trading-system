using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IOperatorSlotService
{
    Task<OperatorSlotsResult> GetSlotsAsync(
        string? operatorUserId,
        CancellationToken cancellationToken = default);

    Task<OperatorSlotUpdateResult> SetAvailabilityAsync(
        string? operatorUserId,
        string slotId,
        bool isAvailable,
        CancellationToken cancellationToken = default);
}

public enum OperatorSlotStatus
{
    Success,
    AuthenticationRequired,
    AccessDenied,
    OperatorStationNotAssigned,
    InvalidSlotId,
    SlotNotFound,
    SlotHasActiveReservation
}

public sealed record OperatorSlotsResult(
    OperatorSlotStatus Status,
    IReadOnlyList<EnergyBookingSlotResponse>? Slots = null);

public sealed record OperatorSlotUpdateResult(
    OperatorSlotStatus Status,
    EnergyBookingSlotResponse? Slot = null);
