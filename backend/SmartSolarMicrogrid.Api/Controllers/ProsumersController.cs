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
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<ProsumerRegistrationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProsumerRegistrationResponse>> Register(
        RegisterProsumerRequest request,
        CancellationToken cancellationToken)
    {
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
    public async Task<ActionResult<IReadOnlyList<ProsumerResponse>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(await managementService.GetAllAsync(cancellationToken));

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpGet("pending")]
    [ProducesResponseType<IReadOnlyList<ProsumerResponse>>(StatusCodes.Status200OK)]
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
        var result = await managementService.ReactivateAsync(prosumerId, cancellationToken);
        return MapManagementResult(result);
    }

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
            _ => Conflict(new ApiErrorResponse(
                "INVALID_ACCOUNT_STATE",
                "The requested account-state transition is not allowed."))
        };
}
