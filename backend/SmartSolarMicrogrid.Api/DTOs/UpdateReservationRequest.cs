// Contains the replacement station and slot selections controlled by a Prosumer.
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class UpdateReservationRequest
{
    [Required]
    public string? StationId { get; init; }

    [Required]
    public string? SlotId { get; init; }
}
