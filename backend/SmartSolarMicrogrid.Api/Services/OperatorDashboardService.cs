using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

// NOTE: Station-scoped counts are a future integration dependency.
// AssignedMicrogridNodeId (ObjectId?) and EnergyReservation.StationId (string)
// have no finalized mapping contract. Global counts are used until Member 2
// delivers the station lookup and the operator-station contract is agreed.
public sealed class OperatorDashboardService(MongoDbService mongoDbService) : IOperatorDashboardService
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var pendingCount = await mongoDbService.CountByStatusAsync("Pending");
        var approvedFutureCount = await mongoDbService.CountApprovedFutureAsync();

        return new DashboardSummaryResponse(pendingCount, approvedFutureCount);
    }
}
