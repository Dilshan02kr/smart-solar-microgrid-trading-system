namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record EnergyBookingSlotResponse(
    string Id,
    string StationId,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    double CapacityKw,
    DateTime CreatedAt,
    DateTime UpdatedAt);
