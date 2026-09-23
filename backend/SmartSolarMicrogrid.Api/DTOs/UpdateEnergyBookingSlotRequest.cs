using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class UpdateEnergyBookingSlotRequest
{
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
