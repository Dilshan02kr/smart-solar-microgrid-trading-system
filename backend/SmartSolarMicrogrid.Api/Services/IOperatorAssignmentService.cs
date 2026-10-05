/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IOperatorAssignmentService.cs
 * Component: Grid Operator Workflow
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Defines the current-database lookup contract used to authorize Grid Operators by station.
 */
namespace SmartSolarMicrogrid.Api.Services;

public interface IOperatorAssignmentService
{
    // Resolve the authenticated operator's current assigned station from UserDetails.
    Task<OperatorAssignmentResult> ResolveAsync(
        string? operatorUserId,
        CancellationToken cancellationToken = default);
}

public enum OperatorAssignmentStatus
{
    Success,
    AuthenticationRequired,
    AccessDenied,
    StationNotAssigned
}

public sealed record OperatorAssignmentResult(
    OperatorAssignmentStatus Status,
    string? StationId = null);
