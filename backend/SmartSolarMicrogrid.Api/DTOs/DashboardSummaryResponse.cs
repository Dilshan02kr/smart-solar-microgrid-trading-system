/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: DashboardSummaryResponse.cs
 * Component: Grid Operator Dashboard
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Returns station-scoped pending and future-approved reservation counts.
 */
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record DashboardSummaryResponse(
    long PendingReservationCount,
    long ApprovedFutureReservationCount);
