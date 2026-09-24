// Exposes role-restricted reservation lifecycle endpoints using authenticated identity.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.Api.Common;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Services;

namespace SmartSolarMicrogrid.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public sealed class ReservationsController(IReservationService reservationService) : ControllerBase
{
    [Authorize(Roles = nameof(UserRole.PROSUMER))]
    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<ReservationResponse>>> GetMine(
        CancellationToken cancellationToken)
    {
        // Return only reservations owned by the authenticated Prosumer.
        var result = await reservationService.GetMineAsync(GetUserId(), cancellationToken);
        return result.Status == ReservationOperationStatus.Success
            ? Ok(result.Reservations)
            : MapListResult(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpGet("prosumer/{prosumerId}")]
    public async Task<ActionResult<IReadOnlyList<ReservationResponse>>> GetByProsumer(
        string prosumerId,
        CancellationToken cancellationToken)
    {
        // Return one Prosumer's reservations for Backoffice administration only.
        var result = await reservationService.GetByProsumerAsync(prosumerId, cancellationToken);
        return result.Status == ReservationOperationStatus.Success
            ? Ok(result.Reservations)
            : MapListResult(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<ReservationResponse>>> Search(
        [FromQuery] string? prosumerId,
        [FromQuery] string? status,
        [FromQuery] string? searchTerm,
        CancellationToken cancellationToken)
    {
        // Search reservations through validated and escaped administrative filters.
        var result = await reservationService.SearchAsync(
            prosumerId,
            status,
            searchTerm,
            cancellationToken);
        return result.Status == ReservationOperationStatus.Success
            ? Ok(result.Reservations)
            : MapListResult(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE) + "," + nameof(UserRole.PROSUMER))]
    [HttpGet("{reservationId}")]
    public async Task<ActionResult<ReservationResponse>> GetById(
        string reservationId,
        CancellationToken cancellationToken)
    {
        // Return reservation details to Backoffice or the owning Prosumer.
        var result = await reservationService.GetByIdAsync(
            reservationId,
            GetUserId(),
            User.IsInRole(nameof(UserRole.BACKOFFICE)),
            cancellationToken);
        return MapSingleResult(result);
    }

    [Authorize(Roles = nameof(UserRole.PROSUMER))]
    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create(
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        // Create a pending reservation using the authenticated Prosumer identity.
        var result = await reservationService.CreateAsync(GetUserId(), request, cancellationToken);
        return result.Status == ReservationOperationStatus.Success
            ? CreatedAtAction(
                nameof(GetById),
                new { reservationId = result.Reservation!.ReservationId },
                result.Reservation)
            : MapSingleResult(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpPatch("{reservationId}/approve")]
    public async Task<ActionResult<ReservationResponse>> Approve(
        string reservationId,
        CancellationToken cancellationToken)
    {
        // Approve a valid future pending reservation and issue its transaction reference.
        var result = await reservationService.ApproveAsync(reservationId, cancellationToken);
        return MapSingleResult(result);
    }

    [Authorize(Roles = nameof(UserRole.PROSUMER))]
    [HttpPut("{reservationId}")]
    public async Task<ActionResult<ReservationResponse>> Update(
        string reservationId,
        UpdateReservationRequest request,
        CancellationToken cancellationToken)
    {
        // Replace an owned reservation's selected station and slot before the cutoff.
        var result = await reservationService.UpdateAsync(
            reservationId,
            GetUserId(),
            request,
            cancellationToken);
        return MapSingleResult(result);
    }

    [Authorize(Roles = nameof(UserRole.PROSUMER))]
    [HttpPut("{reservationId}/cancel")]
    public async Task<ActionResult<ReservationResponse>> Cancel(
        string reservationId,
        CancellationToken cancellationToken)
    {
        // Cancel an owned pending or approved reservation before the cutoff.
        var result = await reservationService.CancelAsync(
            reservationId,
            GetUserId(),
            cancellationToken);
        return MapSingleResult(result);
    }

    private string GetUserId()
    {
        // Read the user identifier established by validated JWT authentication.
        return User.FindFirst("userId")?.Value ?? string.Empty;
    }

    private ActionResult<ReservationResponse> MapSingleResult(ReservationOperationResult result)
    {
        // Convert reservation service outcomes into structured single-resource responses.
        return result.Status switch
        {
            ReservationOperationStatus.Success => Ok(result.Reservation),
            ReservationOperationStatus.InvalidUserId => Unauthorized(Error(
                AuthenticationErrorCodes.AuthenticationRequired,
                "A valid authenticated user ID is required.")),
            ReservationOperationStatus.InvalidReservationId => BadRequest(Error(
                ReservationErrorCodes.InvalidReservationId,
                "The reservation ID must be a valid MongoDB ObjectId.")),
            ReservationOperationStatus.InvalidProsumerId => BadRequest(Error(
                ReservationErrorCodes.InvalidProsumerId,
                "The Prosumer ID must be a valid MongoDB ObjectId.")),
            ReservationOperationStatus.InvalidStationId => BadRequest(Error(
                ReservationErrorCodes.InvalidStationId,
                "The station ID must be a valid MongoDB ObjectId.")),
            ReservationOperationStatus.InvalidSlotId => BadRequest(Error(
                ReservationErrorCodes.InvalidSlotId,
                "The slot ID must be a valid MongoDB ObjectId.")),
            ReservationOperationStatus.InvalidReservationStatus => BadRequest(Error(
                ReservationErrorCodes.InvalidReservationStatus,
                "The reservation status filter is invalid.")),
            ReservationOperationStatus.ReservationNotFound => NotFound(Error(
                ReservationErrorCodes.ReservationNotFound,
                "The requested reservation was not found.")),
            ReservationOperationStatus.StationNotFound => NotFound(Error(
                ReservationErrorCodes.StationNotFound,
                "The referenced station was not found.")),
            ReservationOperationStatus.SlotNotFound => NotFound(Error(
                ReservationErrorCodes.SlotNotFound,
                "The referenced slot was not found.")),
            ReservationOperationStatus.StationInactive => Conflict(Error(
                ReservationErrorCodes.StationInactive,
                "Reservations cannot use an inactive station.")),
            ReservationOperationStatus.SlotNotAvailable => Conflict(Error(
                ReservationErrorCodes.SlotNotAvailable,
                "The selected slot is not available.")),
            ReservationOperationStatus.SlotStationMismatch => BadRequest(Error(
                ReservationErrorCodes.SlotStationMismatch,
                "The selected slot does not belong to the selected station.")),
            ReservationOperationStatus.OutsideAllowedWindow => BadRequest(Error(
                ReservationErrorCodes.ReservationOutsideAllowedWindow,
                "The selected slot must start after now and within seven days.")),
            ReservationOperationStatus.ChangeTooLate => BadRequest(Error(
                ReservationErrorCodes.ReservationChangeTooLate,
                "Reservation changes require at least twelve hours notice.")),
            ReservationOperationStatus.InvalidReservationState => Conflict(Error(
                ReservationErrorCodes.InvalidReservationState,
                "The reservation cannot perform this lifecycle transition.")),
            ReservationOperationStatus.AccessDenied => StatusCode(
                StatusCodes.Status403Forbidden,
                Error(AuthenticationErrorCodes.AccessDenied,
                    "The authenticated account cannot access this reservation.")),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                Error("SERVER_ERROR", "An unexpected error occurred."))
        };
    }

    private ActionResult<IReadOnlyList<ReservationResponse>> MapListResult(
        ReservationOperationResult result)
    {
        // Reuse structured outcome mapping for list-operation failures.
        var singleResult = MapSingleResult(result);
        return new ActionResult<IReadOnlyList<ReservationResponse>>(singleResult.Result!);
    }

    private static ApiErrorResponse Error(string code, string message)
    {
        // Construct the established structured API error contract.
        return new ApiErrorResponse(code, message);
    }
}
