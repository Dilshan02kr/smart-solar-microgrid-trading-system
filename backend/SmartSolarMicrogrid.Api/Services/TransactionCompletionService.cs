// Completes approved reservations within the authenticated operator's assigned station.
using MongoDB.Bson;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class TransactionCompletionService(
    IEnergyReservationRepository reservationRepository,
    IOperatorAssignmentService operatorAssignmentService) : ITransactionCompletionService
{
    public async Task<TransactionCompletionResult> CompleteReservationAsync(
        string? operatorUserId,
        string? reservationId,
        CancellationToken cancellationToken = default)
    {
        // Authorize the current station before atomically completing an approved reservation.
        var assignment = await operatorAssignmentService.ResolveAsync(
            operatorUserId,
            cancellationToken);
        var assignmentFailure = MapAssignmentFailure(assignment.Status);
        if (assignmentFailure.HasValue)
        {
            return new TransactionCompletionResult(assignmentFailure.Value);
        }

        if (string.IsNullOrWhiteSpace(reservationId) ||
            !ObjectId.TryParse(reservationId.Trim(), out _))
        {
            return new TransactionCompletionResult(
                TransactionCompletionStatus.InvalidReservationId);
        }

        var normalizedId = reservationId.Trim();
        var existing = await reservationRepository.GetByIdAsync(normalizedId, cancellationToken);
        if (existing is null)
        {
            return new TransactionCompletionResult(
                TransactionCompletionStatus.ReservationNotFound);
        }

        if (!string.Equals(
                existing.StationId,
                assignment.StationId,
                StringComparison.OrdinalIgnoreCase))
        {
            return new TransactionCompletionResult(TransactionCompletionStatus.AccessDenied);
        }

        var completedAt = DateTime.UtcNow;
        var updatedReservation = await reservationRepository.TryCompleteApprovedAsync(
            normalizedId,
            assignment.StationId!,
            completedAt,
            cancellationToken);

        if (updatedReservation is not null)
        {
            var response = new CompleteReservationResponse(
                updatedReservation.Id,
                updatedReservation.TransactionReference ?? string.Empty,
                updatedReservation.Status.ToString(),
                updatedReservation.ProsumerId,
                updatedReservation.StationId,
                updatedReservation.SlotId,
                updatedReservation.ScheduledTime);

            return new TransactionCompletionResult(
                TransactionCompletionStatus.Success,
                response);
        }

        existing = await reservationRepository.GetByIdAsync(normalizedId, cancellationToken);
        if (existing is null)
        {
            return new TransactionCompletionResult(
                TransactionCompletionStatus.ReservationNotFound);
        }

        if (!string.Equals(
                existing.StationId,
                assignment.StationId,
                StringComparison.OrdinalIgnoreCase))
        {
            return new TransactionCompletionResult(TransactionCompletionStatus.AccessDenied);
        }

        return existing.Status switch
        {
            ReservationStatus.COMPLETED => new TransactionCompletionResult(
                TransactionCompletionStatus.ReservationAlreadyCompleted),
            ReservationStatus.CANCELLED => new TransactionCompletionResult(
                TransactionCompletionStatus.ReservationCancelled),
            _ => new TransactionCompletionResult(
                TransactionCompletionStatus.ReservationNotApproved)
        };
    }

    private static TransactionCompletionStatus? MapAssignmentFailure(
        OperatorAssignmentStatus status)
    {
        // Convert assignment-resolution failures into completion outcomes.
        return status switch
        {
            OperatorAssignmentStatus.AuthenticationRequired =>
                TransactionCompletionStatus.AuthenticationRequired,
            OperatorAssignmentStatus.AccessDenied =>
                TransactionCompletionStatus.AccessDenied,
            OperatorAssignmentStatus.StationNotAssigned =>
                TransactionCompletionStatus.OperatorStationNotAssigned,
            _ => null
        };
    }
}
