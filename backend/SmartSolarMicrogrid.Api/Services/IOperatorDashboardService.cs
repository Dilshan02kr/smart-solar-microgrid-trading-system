// Defines station-scoped dashboard aggregation for the current Grid Operator.
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IOperatorDashboardService
{
    // Returns reservation counts scoped to the operator's current assigned station.
    Task<OperatorDashboardResult> GetSummaryAsync(
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
