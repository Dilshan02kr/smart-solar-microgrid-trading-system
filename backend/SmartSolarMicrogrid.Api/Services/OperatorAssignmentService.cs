/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: OperatorAssignmentService.cs
 * Component: Grid Operator Workflow
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Resolves authoritative Grid Operator station assignments from the current user record.
 */
using MongoDB.Bson;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class OperatorAssignmentService(
    IUserDetailsRepository userRepository) : IOperatorAssignmentService
{
    public async Task<OperatorAssignmentResult> ResolveAsync(
        string? operatorUserId,
        CancellationToken cancellationToken = default)
    {
        // Reload the authenticated user so authorization never relies on client input or stale JWT assignment data.
        if (string.IsNullOrWhiteSpace(operatorUserId) ||
            !ObjectId.TryParse(operatorUserId.Trim(), out _))
        {
            return new OperatorAssignmentResult(
                OperatorAssignmentStatus.AuthenticationRequired);
        }

        var user = await userRepository.GetByIdAsync(operatorUserId.Trim(), cancellationToken);
        if (user is null)
        {
            return new OperatorAssignmentResult(
                OperatorAssignmentStatus.AuthenticationRequired);
        }

        if (user.Role != UserRole.GRID_OPERATOR || user.AccountStatus != AccountStatus.ACTIVE)
        {
            return new OperatorAssignmentResult(OperatorAssignmentStatus.AccessDenied);
        }

        if (!user.AssignedMicrogridNodeId.HasValue)
        {
            return new OperatorAssignmentResult(
                OperatorAssignmentStatus.StationNotAssigned);
        }

        return new OperatorAssignmentResult(
            OperatorAssignmentStatus.Success,
            user.AssignedMicrogridNodeId.Value.ToString());
    }
}
