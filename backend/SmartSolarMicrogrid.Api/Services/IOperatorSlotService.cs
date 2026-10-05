/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IOperatorSlotService.cs
 * Component: Operator Energy Slot Workflow
 *
 * Component Owners:
 * - R A K Hansika (IT23140998) - energy-slot availability contract
 * - Kulunu Kasthuri Arachchi (IT23375628) - assigned-node operator workflow contract
 *
 * Purpose:
 * Defines assigned-station slot operations available to Grid Operators.
 */
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IOperatorSlotService
{
    // Returns slots belonging to the authenticated operator's assigned station.
    Task<OperatorSlotsResult> GetSlotsAsync(
        string? operatorUserId,
        CancellationToken cancellationToken = default);

    // Updates availability for a slot within the authenticated operator's assigned station.
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
