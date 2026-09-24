// Exposes Prosumer registration, self-service profile, and Backoffice lifecycle operations.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.Api.Common;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Services;

namespace SmartSolarMicrogrid.Api.Controllers;

[ApiController]
[Route("api/prosumers")]
public sealed class ProsumersController(
    IProsumerRegistrationService registrationService,
    IProsumerManagementService managementService) : ControllerBase
{
    [Authorize(Roles = nameof(UserRole.PROSUMER))]
    [HttpGet("me")]
    public async Task<ActionResult<ProsumerResponse>> GetMe(
        CancellationToken cancellationToken)
    {
        // Return the authenticated Prosumer's own profile without accepting a user ID.
        return MapManagementResult(await managementService.GetMeAsync(
            GetUserId(),
            cancellationToken));
    }

    [Authorize(Roles = nameof(UserRole.PROSUMER))]
    [HttpPut("me")]
    public async Task<ActionResult<ProsumerResponse>> UpdateMe(
        UpdateProsumerProfileRequest request,
        CancellationToken cancellationToken)
    {
        // Update only fields allowed by the Prosumer self-service contract.
        return MapManagementResult(await managementService.UpdateMeAsync(
            GetUserId(),
            request,
            cancellationToken));
    }

    [Authorize(Roles = nameof(UserRole.PROSUMER))]
    [HttpPatch("me/deactivate")]
    public async Task<ActionResult<ProsumerResponse>> DeactivateMe(
        CancellationToken cancellationToken)
    {
        // Deactivate the authenticated Prosumer so the current token becomes unusable.
        return MapManagementResult(await managementService.DeactivateMeAsync(
            GetUserId(),
            cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<ProsumerRegistrationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProsumerRegistrationResponse>> Register(
        RegisterProsumerRequest request,
        CancellationToken cancellationToken)
    {
        // Registers a Prosumer with a server-controlled pending account status.
        var result = await registrationService.RegisterAsync(request, cancellationToken);

        return result.Status switch
        {
            ProsumerRegistrationStatus.Created =>
                StatusCode(StatusCodes.Status201Created, result.Prosumer),
            ProsumerRegistrationStatus.Invalid =>
                BadRequest(new ApiErrorResponse("VALIDATION_ERROR", result.Message!)),
            ProsumerRegistrationStatus.NicAlreadyExists =>
                Conflict(new ApiErrorResponse(
                    "NIC_ALREADY_EXISTS",
                    "A user with this NIC already exists.")),
            ProsumerRegistrationStatus.EmailAlreadyExists =>
                Conflict(new ApiErrorResponse(
                    "EMAIL_ALREADY_EXISTS",
                    "A user with this email already exists.")),
            _ => Conflict(new ApiErrorResponse(
                "REGISTRATION_CONFLICT",
                result.Message ?? "The registration conflicts with an existing user."))
        };
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProsumerResponse>>(StatusCodes.Status200OK)]
    // Returns every Prosumer account for Backoffice administration.
    public async Task<ActionResult<IReadOnlyList<ProsumerResponse>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(await managementService.GetAllAsync(cancellationToken));

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpGet("pending")]
    [ProducesResponseType<IReadOnlyList<ProsumerResponse>>(StatusCodes.Status200OK)]
    // Returns Prosumer accounts awaiting Backoffice activation.
    public async Task<ActionResult<IReadOnlyList<ProsumerResponse>>> GetPending(
        CancellationToken cancellationToken) =>
        Ok(await managementService.GetPendingAsync(cancellationToken));

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpGet("{prosumerId}")]
    [ProducesResponseType<ProsumerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProsumerResponse>> GetById(
        string prosumerId,
        CancellationToken cancellationToken)
    {
        // Returns one validated Prosumer account to Backoffice.
        var result = await managementService.GetByIdAsync(prosumerId, cancellationToken);
        return MapManagementResult(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpPatch("{prosumerId}/activate")]
    [ProducesResponseType<ProsumerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProsumerResponse>> Activate(
        string prosumerId,
        CancellationToken cancellationToken)
    {
        // Activates a pending Prosumer account through the controlled lifecycle transition.
        var result = await managementService.ActivateAsync(prosumerId, cancellationToken);
        return MapManagementResult(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpPatch("{prosumerId}/reactivate")]
    [ProducesResponseType<ProsumerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProsumerResponse>> Reactivate(
        string prosumerId,
        CancellationToken cancellationToken)
    {
        // Reactivates a previously deactivated Prosumer account as Backoffice.
        var result = await managementService.ReactivateAsync(prosumerId, cancellationToken);
        return MapManagementResult(result);
    }

    // Maps Prosumer service outcomes to safe HTTP responses.
    private ActionResult<ProsumerResponse> MapManagementResult(
        ProsumerManagementResult result) =>
        result.Status switch
        {
            ProsumerManagementStatus.Success => Ok(result.Prosumer),
            ProsumerManagementStatus.InvalidId => BadRequest(new ApiErrorResponse(
                "INVALID_PROSUMER_ID",
                "The Prosumer ID must be a valid MongoDB ObjectId.")),
            ProsumerManagementStatus.NotFound => NotFound(new ApiErrorResponse(
                "PROSUMER_NOT_FOUND",
                "The requested Prosumer was not found.")),
            ProsumerManagementStatus.InvalidAccountState => Conflict(new ApiErrorResponse(
                "INVALID_ACCOUNT_STATE",
                "The requested account-state transition is not allowed.")),
            ProsumerManagementStatus.EmailAlreadyExists => Conflict(new ApiErrorResponse(
                "EMAIL_ALREADY_EXISTS",
                "A user with this email already exists.")),
            ProsumerManagementStatus.ValidationError => BadRequest(new ApiErrorResponse(
                "VALIDATION_ERROR",
                result.ErrorMessage ?? "The supplied profile data is invalid.")),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiErrorResponse("SERVER_ERROR", "An unexpected error occurred."))
        };

    private string GetUserId()
    {
        // Read the user identifier established by validated JWT authentication.
        return User.FindFirst("userId")?.Value ?? string.Empty;
    }
}
