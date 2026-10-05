/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IAuthenticationService.cs
 * Component: Authentication and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines login and current-user operations for centralized authentication.
 */
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IAuthenticationService
{
    // Authenticates a Backoffice or Grid Operator by email and password.
    Task<AuthenticationResult> WebLoginAsync(
        WebLoginRequest request,
        CancellationToken cancellationToken = default);

    // Authenticates a Prosumer by NIC and password.
    Task<AuthenticationResult> ProsumerLoginAsync(
        ProsumerLoginRequest request,
        CancellationToken cancellationToken = default);

    // Returns safe current-user data for an active account.
    Task<AuthenticatedUserResponse?> GetCurrentUserAsync(
        string userId,
        CancellationToken cancellationToken = default);
}

public enum AuthenticationStatus
{
    Success,
    InvalidCredentials,
    AccountPending,
    AccountDeactivated
}

public sealed record AuthenticationResult(
    AuthenticationStatus Status,
    AuthenticationResponse? Response = null);
