// Returns authorized reservation details after transaction-reference verification.
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record VerifyTransactionResponse(
    string ReservationId,
    string TransactionReference,
    string Status,
    string ProsumerId,
    string StationId,
    string SlotId,
    DateTime ScheduledTime);
