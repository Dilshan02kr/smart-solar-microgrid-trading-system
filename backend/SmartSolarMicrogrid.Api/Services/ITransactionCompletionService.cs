using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface ITransactionCompletionService
{
    Task<TransactionCompletionResult> CompleteReservationAsync(
        string? reservationId,
        CancellationToken cancellationToken = default);
}

public enum TransactionCompletionStatus
{
    Success,
    InvalidReservationId,
    ReservationNotFound,
    ReservationNotApproved,
    ReservationCancelled,
    ReservationAlreadyCompleted
}

public sealed record TransactionCompletionResult(
    TransactionCompletionStatus Status,
    CompleteReservationResponse? Response = null);
