/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: ReservationStatus.cs
 * Component: Reservation and Booking Management
 * Component Owner: N A Illangasinghe (IT23391536)
 *
 * Purpose:
 * Defines the complete and authoritative reservation lifecycle states.
 */
namespace SmartSolarMicrogrid.Api.Models;

public enum ReservationStatus
{
    PENDING,
    APPROVED,
    COMPLETED,
    CANCELLED
}
