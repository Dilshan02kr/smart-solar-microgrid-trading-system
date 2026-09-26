// Exposes safe solar-station details through the public API contract.
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record StationResponse(
    string Id,
    string Name,
    string LocationName,
    double Latitude,
    double Longitude,
    double TotalCapacityKw,
    string OperationalSchedule,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
