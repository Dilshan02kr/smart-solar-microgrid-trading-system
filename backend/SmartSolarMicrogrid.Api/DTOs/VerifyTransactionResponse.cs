/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: VerifyTransactionResponse.cs
 * Component: QR Verification and Transaction Completion
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Returns authorized reservation details after transaction-reference verification.
 */
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record VerifyTransactionResponse(
    string ReservationId,
    string TransactionReference,
    string Status,
    string ProsumerId,
    string StationId,
    string SlotId,
    DateTime ScheduledTime);
