// Returns station-scoped pending and future-approved reservation counts.
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record DashboardSummaryResponse(
    long PendingReservationCount,
    long ApprovedFutureReservationCount);
