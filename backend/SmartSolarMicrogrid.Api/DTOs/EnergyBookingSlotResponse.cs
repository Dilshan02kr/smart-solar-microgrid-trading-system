// Exposes safe energy-slot details through the public API contract.
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
