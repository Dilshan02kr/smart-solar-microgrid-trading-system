/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IEnergyBookingSlotService.cs
 * Component: Energy Slot Management
 * Component Owner: R A K Hansika (IT23140998)
 *
 * Purpose:
 * Defines validated business operations for energy booking slots.
 */
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IEnergyBookingSlotService
{
    // Returns all slots as safe API response DTOs.
    Task<IReadOnlyList<EnergyBookingSlotResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // Validates and retrieves one slot.
    Task<EnergyBookingSlotManagementResult> GetByIdAsync(
        string slotId,
        CancellationToken cancellationToken = default);

    // Validates a station and returns its slots.
    Task<EnergyBookingSlotManagementResult> GetByStationIdAsync(
        string stationId,
        CancellationToken cancellationToken = default);

    // Validates and creates a server-controlled slot.
    Task<EnergyBookingSlotManagementResult> CreateAsync(
        CreateEnergyBookingSlotRequest request,
        CancellationToken cancellationToken = default);

    // Validates and updates mutable fields of an existing slot.
    Task<EnergyBookingSlotManagementResult> UpdateAsync(
        string slotId,
        UpdateEnergyBookingSlotRequest request,
        CancellationToken cancellationToken = default);
}

public enum EnergyBookingSlotManagementStatus
{
    Success,
    InvalidSlotId,
    InvalidStationId,
    SlotNotFound,
    StationNotFound,
    InvalidSlotTime,
    InvalidSlotCapacity,
    SlotCapacityExceedsStation,
    SlotTimeConflict
}

public sealed record EnergyBookingSlotManagementResult(
    EnergyBookingSlotManagementStatus Status,
    EnergyBookingSlotResponse? Slot = null,
    IReadOnlyList<EnergyBookingSlotResponse>? Slots = null,
    string? ErrorMessage = null);
