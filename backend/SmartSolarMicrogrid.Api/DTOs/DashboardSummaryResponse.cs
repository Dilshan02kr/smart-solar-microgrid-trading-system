namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record DashboardSummaryResponse(
    long PendingReservationCount,
    long ApprovedFutureReservationCount);
