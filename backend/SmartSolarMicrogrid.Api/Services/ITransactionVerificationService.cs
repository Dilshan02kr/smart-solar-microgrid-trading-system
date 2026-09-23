using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface ITransactionVerificationService
{
    Task<TransactionVerificationResult> VerifyTransactionAsync(
        string? transactionReference,
        CancellationToken cancellationToken = default);
}

public enum TransactionVerificationStatus
{
    Success,
    InvalidTransactionReference,
    ReservationNotFound,
    ReservationNotApproved,
    ReservationCancelled,
    ReservationAlreadyCompleted
}

public sealed record TransactionVerificationResult(
    TransactionVerificationStatus Status,
    VerifyTransactionResponse? Response = null);
