// Exposes the minimum reservation context needed for assigned-station operations.
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record OperatorReservationResponse(
    string ReservationId,
    string ProsumerId,
    string SlotId,
    DateTime ScheduledTime,
    string Status);
