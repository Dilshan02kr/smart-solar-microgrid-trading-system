// Defines validated client fields for creating a solar station.
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class CreateStationRequest
{
    [Required(ErrorMessage = "Station name is required.")]
    public string? Name { get; init; }

    [Required(ErrorMessage = "Location name is required.")]
    public string? LocationName { get; init; }

    [Required(ErrorMessage = "Latitude is required.")]
    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and +90 degrees.")]
    public double? Latitude { get; init; }

    [Required(ErrorMessage = "Longitude is required.")]
    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and +180 degrees.")]
    public double? Longitude { get; init; }

    [Required(ErrorMessage = "Total capacity is required.")]
    [Range(0.0, double.MaxValue, MinimumIsExclusive = true, ErrorMessage = "Total capacity must be greater than 0 kW.")]
    public double? TotalCapacityKw { get; init; }

    [Required(ErrorMessage = "Operational schedule is required.")]
    public string? OperationalSchedule { get; init; }
}
