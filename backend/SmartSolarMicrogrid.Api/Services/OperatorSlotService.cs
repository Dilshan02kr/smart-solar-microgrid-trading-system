/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: OperatorSlotService.cs
 * Component: Operator Energy Slot Workflow
 *
 * Component Owners:
 * - R A K Hansika (IT23140998) - energy-slot availability rules
 * - Kulunu Kasthuri Arachchi (IT23375628) - assigned-node operator workflow
 *
 * Purpose:
 * Provides assigned-station slot retrieval and availability updates for Grid Operators.
 */
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
        // Returns slots scoped to the authenticated operator's current station assignment.
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
        // Changes assigned-station slot availability while protecting active reservations.
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

    // Maps assignment authorization outcomes to operator-slot service statuses.
    private static OperatorSlotStatus MapAssignmentStatus(OperatorAssignmentStatus status) => status switch
    {
        OperatorAssignmentStatus.AuthenticationRequired => OperatorSlotStatus.AuthenticationRequired,
        OperatorAssignmentStatus.StationNotAssigned => OperatorSlotStatus.OperatorStationNotAssigned,
        _ => OperatorSlotStatus.AccessDenied
    };

    // Maps a persisted energy slot to the operator-facing response contract.
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
