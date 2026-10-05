using MongoDB.Bson;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class OperatorSlotService(
    IEnergyBookingSlotRepository slotRepository,
    IEnergyReservationRepository reservationRepository,
    IOperatorAssignmentService operatorAssignmentService) : IOperatorSlotService
{
    public async Task<OperatorSlotsResult> GetSlotsAsync(
        string? operatorUserId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await operatorAssignmentService.ResolveAsync(operatorUserId, cancellationToken);
        if (assignment.Status != OperatorAssignmentStatus.Success)
        {
            return new OperatorSlotsResult(MapAssignmentStatus(assignment.Status));
        }

        var slots = await slotRepository.GetByStationIdAsync(assignment.StationId!, cancellationToken);
        return new OperatorSlotsResult(
            OperatorSlotStatus.Success,
            slots.Select(ToResponse).ToList());
    }

    public async Task<OperatorSlotUpdateResult> SetAvailabilityAsync(
        string? operatorUserId,
        string slotId,
        bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        var assignment = await operatorAssignmentService.ResolveAsync(operatorUserId, cancellationToken);
        if (assignment.Status != OperatorAssignmentStatus.Success)
        {
            return new OperatorSlotUpdateResult(MapAssignmentStatus(assignment.Status));
        }

        if (!ObjectId.TryParse(slotId, out _))
        {
            return new OperatorSlotUpdateResult(OperatorSlotStatus.InvalidSlotId);
        }

        var slot = await slotRepository.GetByIdAsync(slotId, cancellationToken);
        if (slot is null)
        {
            return new OperatorSlotUpdateResult(OperatorSlotStatus.SlotNotFound);
        }

        if (!string.Equals(slot.StationId.ToString(), assignment.StationId, StringComparison.Ordinal))
        {
            return new OperatorSlotUpdateResult(OperatorSlotStatus.AccessDenied);
        }

        if (isAvailable && await reservationRepository.HasActiveReservationForSlotAsync(slotId, cancellationToken))
        {
            return new OperatorSlotUpdateResult(OperatorSlotStatus.SlotHasActiveReservation);
        }

        var updated = await slotRepository.SetAvailabilityAsync(slotId, isAvailable, cancellationToken);
        return updated is null
            ? new OperatorSlotUpdateResult(OperatorSlotStatus.SlotNotFound)
            : new OperatorSlotUpdateResult(OperatorSlotStatus.Success, ToResponse(updated));
    }

    private static OperatorSlotStatus MapAssignmentStatus(OperatorAssignmentStatus status) => status switch
    {
        OperatorAssignmentStatus.AuthenticationRequired => OperatorSlotStatus.AuthenticationRequired,
        OperatorAssignmentStatus.StationNotAssigned => OperatorSlotStatus.OperatorStationNotAssigned,
        _ => OperatorSlotStatus.AccessDenied
    };

    private static EnergyBookingSlotResponse ToResponse(Models.EnergyBookingSlot slot) => new(
        slot.Id.ToString(),
        slot.StationId.ToString(),
        slot.Date,
        slot.StartTime,
        slot.EndTime,
        slot.CapacityKw,
        slot.IsAvailable,
        slot.CreatedAt,
        slot.UpdatedAt);
}
