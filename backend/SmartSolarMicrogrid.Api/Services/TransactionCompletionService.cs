using MongoDB.Bson;
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class TransactionCompletionService(MongoDbService mongoDbService) : ITransactionCompletionService
{
    public async Task<TransactionCompletionResult> CompleteReservationAsync(
        string? reservationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reservationId) || !ObjectId.TryParse(reservationId.Trim(), out _))
        {
            return new TransactionCompletionResult(
                TransactionCompletionStatus.InvalidReservationId);
        }

        var normalizedId = reservationId.Trim();

        // 1. Attempt atomic conditional transition from "Approved" to "Completed"
        // This guarantees that concurrent completion attempts cannot both succeed.
        var updatedReservation = await mongoDbService.TryCompleteApprovedReservationAsync(normalizedId);

        if (updatedReservation is not null)
        {
            var response = new CompleteReservationResponse(
                updatedReservation.Id ?? normalizedId,
                updatedReservation.TransactionReference,
                updatedReservation.Status,
                updatedReservation.ProsumerId,
                updatedReservation.StationId,
                updatedReservation.SlotId,
                updatedReservation.ScheduledTime);

            return new TransactionCompletionResult(
                TransactionCompletionStatus.Success,
                response);
        }

        // 2. If the atomic update returned null, inspect the current state to return the exact failure reason
        var existing = await mongoDbService.GetByIdAsync(normalizedId);
        if (existing is null)
        {
            return new TransactionCompletionResult(
                TransactionCompletionStatus.ReservationNotFound);
        }

        if (string.Equals(existing.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            return new TransactionCompletionResult(
                TransactionCompletionStatus.ReservationAlreadyCompleted);
        }

        if (string.Equals(existing.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            return new TransactionCompletionResult(
                TransactionCompletionStatus.ReservationCancelled);
        }

        if (string.Equals(existing.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return new TransactionCompletionResult(
                TransactionCompletionStatus.ReservationNotApproved);
        }

        return new TransactionCompletionResult(
            TransactionCompletionStatus.ReservationNotApproved);
    }
}
