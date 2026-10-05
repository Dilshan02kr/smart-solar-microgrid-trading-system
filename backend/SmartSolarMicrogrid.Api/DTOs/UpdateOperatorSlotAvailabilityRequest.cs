using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

// Defines the sole operator-controlled field for an existing energy slot.
public sealed class UpdateOperatorSlotAvailabilityRequest
{
    [Required]
    public bool? IsAvailable { get; init; }
}
