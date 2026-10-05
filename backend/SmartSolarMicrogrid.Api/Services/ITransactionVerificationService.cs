/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: ITransactionVerificationService.cs
 * Component: QR Verification and Transaction Completion
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Defines station-authorized verification of reservation transaction references.
 */
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface ITransactionVerificationService
{
    // Verifies an approved transaction reference within the operator's assigned station.
    Task<TransactionVerificationResult> VerifyTransactionAsync(
        string? operatorUserId,
        string? transactionReference,
        CancellationToken cancellationToken = default);
}

public enum TransactionVerificationStatus
{
    Success,
    AuthenticationRequired,
    AccessDenied,
    OperatorStationNotAssigned,
    InvalidTransactionReference,
    ReservationNotFound,
    ReservationNotApproved,
    ReservationCancelled,
    ReservationAlreadyCompleted
}

public sealed record TransactionVerificationResult(
    TransactionVerificationStatus Status,
    VerifyTransactionResponse? Response = null);
