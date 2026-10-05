/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: CompleteReservationResponse.cs
 * Component: QR Verification and Transaction Completion
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Returns safe reservation details after an operator completes a transaction.
 */
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record CompleteReservationResponse(
    string ReservationId,
    string TransactionReference,
    string Status,
    string ProsumerId,
    string StationId,
    string SlotId,
    DateTime ScheduledTime);
