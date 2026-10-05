/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: OperatorDashboardService.cs
 * Component: Grid Operator Dashboard
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Produces pending and future-approved counts for the operator's assigned station.
 */
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class OperatorDashboardService(
    IEnergyReservationRepository reservationRepository,
    IOperatorAssignmentService operatorAssignmentService) : IOperatorDashboardService
{
    public async Task<OperatorDashboardResult> GetSummaryAsync(
        string? operatorUserId,
        CancellationToken cancellationToken = default)
    {
        // Scope pending and future-approved counts to the operator's current assigned station.
        var assignment = await operatorAssignmentService.ResolveAsync(
            operatorUserId,
            cancellationToken);
        if (assignment.Status != OperatorAssignmentStatus.Success)
        {
            return new OperatorDashboardResult(assignment.Status switch
            {
                OperatorAssignmentStatus.AuthenticationRequired =>
                    OperatorDashboardStatus.AuthenticationRequired,
                OperatorAssignmentStatus.StationNotAssigned =>
                    OperatorDashboardStatus.OperatorStationNotAssigned,
                _ => OperatorDashboardStatus.AccessDenied
            });
        }

        var pendingCount = await reservationRepository.CountByStationAndStatusAsync(
            assignment.StationId!,
            ReservationStatus.PENDING,
            cancellationToken);
        var approvedFutureCount = await reservationRepository.CountApprovedFutureByStationAsync(
            assignment.StationId!,
            DateTime.UtcNow,
            cancellationToken);

        return new OperatorDashboardResult(
            OperatorDashboardStatus.Success,
            new DashboardSummaryResponse(pendingCount, approvedFutureCount));
    }

    public async Task<OperatorReservationsResult> GetReservationsAsync(
        string? operatorUserId,
        CancellationToken cancellationToken = default)
    {
        // Resolve the current database assignment; no station identifier is accepted from the client.
        var assignment = await operatorAssignmentService.ResolveAsync(operatorUserId, cancellationToken);
        if (assignment.Status != OperatorAssignmentStatus.Success)
        {
            return new OperatorReservationsResult(MapAssignmentStatus(assignment.Status));
        }

        var reservations = await reservationRepository.GetOperationalByStationAsync(
            assignment.StationId!,
            cancellationToken);
        return new OperatorReservationsResult(
            OperatorDashboardStatus.Success,
            reservations.Select(reservation => new OperatorReservationResponse(
                reservation.Id,
                reservation.ProsumerId,
                reservation.SlotId,
                reservation.ScheduledTime,
                reservation.Status.ToString())).ToList());
    }

    // Maps assignment authorization outcomes to operator-dashboard service statuses.
    private static OperatorDashboardStatus MapAssignmentStatus(OperatorAssignmentStatus status) => status switch
    {
        OperatorAssignmentStatus.AuthenticationRequired => OperatorDashboardStatus.AuthenticationRequired,
        OperatorAssignmentStatus.StationNotAssigned => OperatorDashboardStatus.OperatorStationNotAssigned,
        _ => OperatorDashboardStatus.AccessDenied
    };
}
