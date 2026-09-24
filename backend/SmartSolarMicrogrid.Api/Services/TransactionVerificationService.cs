using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class TransactionVerificationService(MongoDbService mongoDbService) : ITransactionVerificationService
{
    public async Task<TransactionVerificationResult> VerifyTransactionAsync(
        string? transactionReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionReference))
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.InvalidTransactionReference);
        }

        var normalizedReference = transactionReference.Trim();

        var reservation = await mongoDbService.GetByTransactionReferenceAsync(normalizedReference);
        if (reservation is null)
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.ReservationNotFound);
        }

        if (string.Equals(reservation.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.ReservationCancelled);
        }

        if (string.Equals(reservation.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.ReservationAlreadyCompleted);
        }

        if (string.Equals(reservation.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.ReservationNotApproved);
        }

        if (!string.Equals(reservation.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.ReservationNotApproved);
        }

        var response = new VerifyTransactionResponse(
            reservation.Id ?? string.Empty,
            reservation.TransactionReference,
            reservation.Status,
            reservation.ProsumerId,
            reservation.StationId,
            reservation.SlotId,
            reservation.ScheduledTime);

        return new TransactionVerificationResult(
            TransactionVerificationStatus.Success,
            response);
    }
}
