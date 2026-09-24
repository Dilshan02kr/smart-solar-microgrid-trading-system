// Defines station-authorized completion of approved reservations.
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface ITransactionCompletionService
{
    // Completes an approved reservation within the operator's assigned station.
    Task<TransactionCompletionResult> CompleteReservationAsync(
        string? operatorUserId,
        string? reservationId,
        CancellationToken cancellationToken = default);
}

public enum TransactionCompletionStatus
{
    Success,
    AuthenticationRequired,
    AccessDenied,
    OperatorStationNotAssigned,
    InvalidReservationId,
    ReservationNotFound,
    ReservationNotApproved,
    ReservationCancelled,
    ReservationAlreadyCompleted
}

public sealed record TransactionCompletionResult(
    TransactionCompletionStatus Status,
    CompleteReservationResponse? Response = null);
