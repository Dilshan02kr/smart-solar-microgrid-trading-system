/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: ReservationResponse.cs
 * Component: Reservation and Booking Management
 * Component Owner: N A Illangasinghe (IT23391536)
 *
 * Purpose:
 * Exposes reservation data without leaking persistence implementation details.
 */
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record ReservationResponse(
    string ReservationId,
    string ProsumerId,
    string StationId,
    string SlotId,
    DateTime ScheduledTime,
    string Status,
    string? TransactionReference,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt);
