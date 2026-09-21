using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.Api.Common;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Services;

namespace SmartSolarMicrogrid.Api.Controllers;

[ApiController]
[Route("api/prosumers")]
public sealed class ProsumersController(
    IProsumerRegistrationService registrationService) : ControllerBase
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
}
