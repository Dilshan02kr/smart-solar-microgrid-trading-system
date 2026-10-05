/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: AuthController.cs
 * Component: Authentication and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Exposes login and current-user endpoints backed by centralized authentication services.
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.Api.Common;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Services;

namespace SmartSolarMicrogrid.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthenticationService authenticationService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("web-login")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status403Forbidden)]
    // Authenticates Backoffice or Grid Operator credentials and returns an access token.
    public Task<ActionResult<AuthenticationResponse>> WebLogin(
        WebLoginRequest request,
        CancellationToken cancellationToken) =>
        LoginAsync(authenticationService.WebLoginAsync(request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("prosumer-login")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status403Forbidden)]
    // Authenticates Prosumer NIC credentials and returns an access token.
    public Task<ActionResult<AuthenticationResponse>> ProsumerLogin(
        ProsumerLoginRequest request,
        CancellationToken cancellationToken) =>
        LoginAsync(authenticationService.ProsumerLoginAsync(request, cancellationToken));

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<AuthenticatedUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticatedUserResponse>> Me(
        CancellationToken cancellationToken)
    {
        // Returns the current active user's safe identity from the validated JWT subject.
        var userId = User.FindFirst("userId")?.Value;
        var user = string.IsNullOrWhiteSpace(userId)
            ? null
            : await authenticationService.GetCurrentUserAsync(userId, cancellationToken);

        return user is null
            ? Unauthorized(new ApiErrorResponse(
                AuthenticationErrorCodes.AuthenticationRequired,
                "A valid authentication token is required."))
            : Ok(user);
    }

    // Maps authentication outcomes to consistent success or structured error responses.
    private ActionResult<AuthenticationResponse> LoginAsyncResult(AuthenticationResult result) =>
        result.Status switch
        {
            AuthenticationStatus.Success => Ok(result.Response),
            AuthenticationStatus.AccountPending => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiErrorResponse(
                    AuthenticationErrorCodes.AccountPending,
                    "The account is pending activation.")),
            AuthenticationStatus.AccountDeactivated => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiErrorResponse(
                    AuthenticationErrorCodes.AccountDeactivated,
                    "The account is deactivated.")),
            _ => Unauthorized(new ApiErrorResponse(
                AuthenticationErrorCodes.InvalidCredentials,
                "The supplied credentials are invalid."))
        };

    // Awaits a login operation and applies the shared HTTP result mapping.
    private async Task<ActionResult<AuthenticationResponse>> LoginAsync(
        Task<AuthenticationResult> authenticationTask) =>
        LoginAsyncResult(await authenticationTask);
}
