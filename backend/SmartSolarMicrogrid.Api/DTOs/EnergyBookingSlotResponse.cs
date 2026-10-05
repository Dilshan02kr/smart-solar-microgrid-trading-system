/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: EnergyBookingSlotResponse.cs
 * Component: Energy Slot Management
 * Component Owner: R A K Hansika (IT23140998)
 *
 * Purpose:
 * Exposes safe energy-slot details through the public API contract.
 */
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record EnergyBookingSlotResponse(
    string Id,
    string StationId,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    double CapacityKw,
    bool IsAvailable,
    DateTime CreatedAt,
    DateTime UpdatedAt);
