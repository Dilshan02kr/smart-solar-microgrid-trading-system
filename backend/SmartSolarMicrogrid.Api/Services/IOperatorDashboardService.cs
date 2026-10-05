/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IOperatorDashboardService.cs
 * Component: Grid Operator Dashboard
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Defines station-scoped dashboard aggregation for the current Grid Operator.
 */
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IOperatorDashboardService
{
    // Returns reservation counts scoped to the operator's current assigned station.
    Task<OperatorDashboardResult> GetSummaryAsync(
        string? operatorUserId,
        CancellationToken cancellationToken = default);

    // Returns pending and approved reservations for the operator's current database assignment.
    Task<OperatorReservationsResult> GetReservationsAsync(
        string? operatorUserId,
        CancellationToken cancellationToken = default);
}

public enum OperatorDashboardStatus
{
    Success,
    AuthenticationRequired,
    AccessDenied,
    OperatorStationNotAssigned
}

public sealed record OperatorDashboardResult(
    OperatorDashboardStatus Status,
    DashboardSummaryResponse? Response = null);

public sealed record OperatorReservationsResult(
    OperatorDashboardStatus Status,
    IReadOnlyList<OperatorReservationResponse>? Reservations = null);
