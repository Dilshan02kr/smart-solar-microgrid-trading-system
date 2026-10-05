/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: OperatorReservationResponse.cs
 * Component: Grid Operator Workflow
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Exposes the minimum reservation context needed for assigned-station operations.
 */
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record OperatorReservationResponse(
    string ReservationId,
    string ProsumerId,
    string SlotId,
    DateTime ScheduledTime,
    string Status);
