/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: UpdateReservationRequest.cs
 * Component: Reservation and Booking Management
 * Component Owner: N A Illangasinghe (IT23391536)
 *
 * Purpose:
 * Contains the replacement station and slot selections controlled by a Prosumer.
 */
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class UpdateReservationRequest
{
    [Required]
    public string? StationId { get; init; }

    [Required]
    public string? SlotId { get; init; }
}
