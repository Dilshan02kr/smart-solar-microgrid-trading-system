using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class CreateEnergyBookingSlotRequest
{
    [Required(ErrorMessage = "Station ID is required.")]
    [RegularExpression(@"^[0-9a-fA-F]{24}$", ErrorMessage = "Station ID must be a valid 24-character hexadecimal string.")]
    public string? StationId { get; init; }

    [Required(ErrorMessage = "Date is required.")]
    public DateTime? Date { get; init; }

    [Required(ErrorMessage = "Start time is required.")]
    public TimeSpan? StartTime { get; init; }

    [Required(ErrorMessage = "End time is required.")]
    public TimeSpan? EndTime { get; init; }

    [Required(ErrorMessage = "Capacity is required.")]
    [Range(0.0, double.MaxValue, MinimumIsExclusive = true, ErrorMessage = "Capacity must be greater than 0 kW.")]
    public double? CapacityKw { get; init; }
}
