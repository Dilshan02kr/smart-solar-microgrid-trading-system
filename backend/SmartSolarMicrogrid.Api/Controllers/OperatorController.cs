// Exposes station-scoped verification, completion, and dashboard operations for Grid Operators.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.Api.Common;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Services;

namespace SmartSolarMicrogrid.Api.Controllers;

[ApiController]
[Route("api/operator")]
[Authorize(Roles = nameof(UserRole.GRID_OPERATOR))]
public sealed class OperatorController(
    ITransactionVerificationService verificationService,
    ITransactionCompletionService completionService,
    IOperatorDashboardService dashboardService) : ControllerBase
{
    [HttpPost("verify-transaction")]
    [ProducesResponseType<VerifyTransactionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VerifyTransactionResponse>> VerifyTransaction(
        VerifyTransactionRequest request,
        CancellationToken cancellationToken)
    {
        // Resolve the operator identity from the validated JWT before station-scoped verification.
        var result = await verificationService.VerifyTransactionAsync(
            User.FindFirst("userId")?.Value,
            request.TransactionReference,
            cancellationToken);

        return result.Status switch
        {
            TransactionVerificationStatus.Success =>
                Ok(result.Response),

            TransactionVerificationStatus.AuthenticationRequired =>
                Unauthorized(new ApiErrorResponse(
                    AuthenticationErrorCodes.AuthenticationRequired,
                    "Authentication is required.")),

            TransactionVerificationStatus.AccessDenied =>
                StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                    AuthenticationErrorCodes.AccessDenied,
                    "The reservation does not belong to the assigned station.")),

            TransactionVerificationStatus.OperatorStationNotAssigned =>
                StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                    "OPERATOR_STATION_NOT_ASSIGNED",
                    "The Grid Operator is not assigned to a station.")),

            TransactionVerificationStatus.InvalidTransactionReference =>
                BadRequest(new ApiErrorResponse(
                    ReservationErrorCodes.InvalidTransactionReference,
                    "The supplied transaction reference is invalid.")),

            TransactionVerificationStatus.ReservationNotFound =>
                NotFound(new ApiErrorResponse(
                    ReservationErrorCodes.ReservationNotFound,
                    "No reservation was found for the supplied transaction reference.")),

            TransactionVerificationStatus.ReservationNotApproved =>
                Conflict(new ApiErrorResponse(
                    ReservationErrorCodes.ReservationNotApproved,
                    "The reservation is not in an approved state.")),

            TransactionVerificationStatus.ReservationCancelled =>
                Conflict(new ApiErrorResponse(
                    ReservationErrorCodes.ReservationCancelled,
                    "The reservation has been cancelled.")),

            TransactionVerificationStatus.ReservationAlreadyCompleted =>
                Conflict(new ApiErrorResponse(
                    ReservationErrorCodes.ReservationAlreadyCompleted,
                    "The reservation has already been completed.")),

            _ => Conflict(new ApiErrorResponse(
                ReservationErrorCodes.ReservationNotApproved,
                "The reservation could not be verified."))
        };
    }

    [HttpPost("reservations/{reservationId}/complete")]
    [ProducesResponseType<CompleteReservationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompleteReservationResponse>> CompleteReservation(
        string reservationId,
        CancellationToken cancellationToken)
    {
        // Resolve the operator identity from the validated JWT before station-scoped completion.
        var result = await completionService.CompleteReservationAsync(
            User.FindFirst("userId")?.Value,
            reservationId,
            cancellationToken);

        return result.Status switch
        {
            TransactionCompletionStatus.Success =>
                Ok(result.Response),

            TransactionCompletionStatus.AuthenticationRequired =>
                Unauthorized(new ApiErrorResponse(
                    AuthenticationErrorCodes.AuthenticationRequired,
                    "Authentication is required.")),

            TransactionCompletionStatus.AccessDenied =>
                StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                    AuthenticationErrorCodes.AccessDenied,
                    "The reservation does not belong to the assigned station.")),

            TransactionCompletionStatus.OperatorStationNotAssigned =>
                StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                    "OPERATOR_STATION_NOT_ASSIGNED",
                    "The Grid Operator is not assigned to a station.")),

            TransactionCompletionStatus.InvalidReservationId =>
                BadRequest(new ApiErrorResponse(
                    ReservationErrorCodes.InvalidReservationId,
                    "The supplied reservation ID is invalid.")),

            TransactionCompletionStatus.ReservationNotFound =>
                NotFound(new ApiErrorResponse(
                    ReservationErrorCodes.ReservationNotFound,
                    "No reservation was found for the supplied reservation ID.")),

            TransactionCompletionStatus.ReservationNotApproved =>
                Conflict(new ApiErrorResponse(
                    ReservationErrorCodes.ReservationNotApproved,
                    "The reservation is not in an approved state.")),

            TransactionCompletionStatus.ReservationCancelled =>
                Conflict(new ApiErrorResponse(
                    ReservationErrorCodes.ReservationCancelled,
                    "The reservation has been cancelled.")),

            TransactionCompletionStatus.ReservationAlreadyCompleted =>
                Conflict(new ApiErrorResponse(
                    ReservationErrorCodes.ReservationAlreadyCompleted,
                    "The reservation has already been completed.")),

            _ => Conflict(new ApiErrorResponse(
                ReservationErrorCodes.ReservationNotApproved,
                "The reservation could not be completed."))
        };
    }

    [HttpGet("dashboard/summary")]
    [ProducesResponseType<DashboardSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardSummaryResponse>> GetDashboardSummary(
        CancellationToken cancellationToken)
    {
        // Return counts scoped to the current operator assignment from UserDetails.
        var result = await dashboardService.GetSummaryAsync(
            User.FindFirst("userId")?.Value,
            cancellationToken);

        return result.Status switch
        {
            OperatorDashboardStatus.Success => Ok(result.Response),
            OperatorDashboardStatus.AuthenticationRequired => Unauthorized(new ApiErrorResponse(
                AuthenticationErrorCodes.AuthenticationRequired,
                "Authentication is required.")),
            OperatorDashboardStatus.OperatorStationNotAssigned =>
                StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                    "OPERATOR_STATION_NOT_ASSIGNED",
                    "The Grid Operator is not assigned to a station.")),
            _ => StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                AuthenticationErrorCodes.AccessDenied,
                "Access is denied."))
        };
    }

    [HttpGet("reservations")]
    [ProducesResponseType<IReadOnlyList<OperatorReservationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<OperatorReservationResponse>>> GetReservations(
        CancellationToken cancellationToken)
    {
        // Station scope comes only from the authenticated operator's current database record.
        var result = await dashboardService.GetReservationsAsync(
            User.FindFirst("userId")?.Value,
            cancellationToken);

        return result.Status switch
        {
            OperatorDashboardStatus.Success => Ok(result.Reservations),
            OperatorDashboardStatus.AuthenticationRequired => Unauthorized(new ApiErrorResponse(
                AuthenticationErrorCodes.AuthenticationRequired,
                "Authentication is required.")),
            OperatorDashboardStatus.OperatorStationNotAssigned =>
                StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                    "OPERATOR_STATION_NOT_ASSIGNED",
                    "The Grid Operator is not assigned to a station.")),
            _ => StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                AuthenticationErrorCodes.AccessDenied,
                "Access is denied."))
        };
    }
}
