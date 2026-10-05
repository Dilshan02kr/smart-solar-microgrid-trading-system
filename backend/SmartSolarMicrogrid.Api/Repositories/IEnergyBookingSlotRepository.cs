/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IEnergyBookingSlotRepository.cs
 * Component: Energy Slot Management
 * Component Owner: R A K Hansika (IT23140998)
 *
 * Purpose:
 * Defines MongoDB persistence operations required by energy-slot business workflows.
 */
using MongoDB.Bson;
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Repositories;

public interface IEnergyBookingSlotRepository
{
    // Inserts a new server-controlled slot document.
    Task CreateAsync(
        EnergyBookingSlot slot,
        CancellationToken cancellationToken = default);

    // Returns all persisted booking slots.
    Task<IReadOnlyList<EnergyBookingSlot>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // Retrieves one slot by a validated public identifier.
    Task<EnergyBookingSlot?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    // Returns slots belonging to one station.
    Task<IReadOnlyList<EnergyBookingSlot>> GetByStationIdAsync(
        string stationId,
        CancellationToken cancellationToken = default);

    // Determines whether a same-station time range overlaps another slot.
    Task<bool> HasOverlappingSlotAsync(
        ObjectId stationId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        ObjectId? excludedSlotId = null,
        CancellationToken cancellationToken = default);

    // Atomically claims a currently available slot.
    Task<bool> TryClaimAvailableAsync(
        string slotId,
        CancellationToken cancellationToken = default);

    // Releases a slot after an eligible reservation cancellation or update.
    Task<bool> ReleaseAsync(
        string slotId,
        CancellationToken cancellationToken = default);

    // Sets only the operational availability flag and returns the authoritative document.
    Task<EnergyBookingSlot?> SetAvailabilityAsync(
        string slotId,
        bool isAvailable,
        CancellationToken cancellationToken = default);

    // Updates only mutable timing and capacity fields of a slot.
    Task<EnergyBookingSlot?> UpdateAsync(
        string id,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        double capacityKw,
        CancellationToken cancellationToken = default);
}
