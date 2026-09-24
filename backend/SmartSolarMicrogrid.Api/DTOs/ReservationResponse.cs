// Exposes reservation data without leaking persistence implementation details.
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record ReservationResponse(
    string ReservationId,
    string ProsumerId,
    string StationId,
    string SlotId,
    DateTime ScheduledTime,
    string Status,
    string? TransactionReference,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt);
