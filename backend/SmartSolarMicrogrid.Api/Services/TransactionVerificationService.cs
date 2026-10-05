/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: TransactionVerificationService.cs
 * Component: QR Verification and Transaction Completion
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Verifies approved transaction references within the operator's assigned station.
 */
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class TransactionVerificationService(
    IEnergyReservationRepository reservationRepository,
    IOperatorAssignmentService operatorAssignmentService) : ITransactionVerificationService
{
    public async Task<TransactionVerificationResult> VerifyTransactionAsync(
        string? operatorUserId,
        string? transactionReference,
        CancellationToken cancellationToken = default)
    {
        // Verify an approved reference only after resolving the operator's current station assignment.
        var assignment = await operatorAssignmentService.ResolveAsync(
            operatorUserId,
            cancellationToken);
        var assignmentFailure = MapAssignmentFailure(assignment.Status);
        if (assignmentFailure.HasValue)
        {
            return new TransactionVerificationResult(assignmentFailure.Value);
        }

        if (string.IsNullOrWhiteSpace(transactionReference))
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.InvalidTransactionReference);
        }

        var reservation = await reservationRepository.GetByTransactionReferenceAsync(
            transactionReference.Trim(),
            cancellationToken);
        if (reservation is null)
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.ReservationNotFound);
        }

        if (!string.Equals(
                reservation.StationId,
                assignment.StationId,
                StringComparison.OrdinalIgnoreCase))
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.AccessDenied);
        }

        if (reservation.Status == ReservationStatus.CANCELLED)
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.ReservationCancelled);
        }

        if (reservation.Status == ReservationStatus.COMPLETED)
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.ReservationAlreadyCompleted);
        }

        if (reservation.Status != ReservationStatus.APPROVED ||
            string.IsNullOrWhiteSpace(reservation.TransactionReference))
        {
            return new TransactionVerificationResult(
                TransactionVerificationStatus.ReservationNotApproved);
        }

        var response = new VerifyTransactionResponse(
            reservation.Id,
            reservation.TransactionReference,
            reservation.Status.ToString(),
            reservation.ProsumerId,
            reservation.StationId,
            reservation.SlotId,
            reservation.ScheduledTime);

        return new TransactionVerificationResult(
            TransactionVerificationStatus.Success,
            response);
    }

    private static TransactionVerificationStatus? MapAssignmentFailure(
        OperatorAssignmentStatus status)
    {
        // Convert assignment-resolution failures into verification outcomes.
        return status switch
        {
            OperatorAssignmentStatus.AuthenticationRequired =>
                TransactionVerificationStatus.AuthenticationRequired,
            OperatorAssignmentStatus.AccessDenied =>
                TransactionVerificationStatus.AccessDenied,
            OperatorAssignmentStatus.StationNotAssigned =>
                TransactionVerificationStatus.OperatorStationNotAssigned,
            _ => null
        };
    }
}
