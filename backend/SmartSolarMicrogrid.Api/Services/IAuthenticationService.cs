using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResult> WebLoginAsync(
        WebLoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResult> ProsumerLoginAsync(
        ProsumerLoginRequest request,
        CancellationToken cancellationToken = default);

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
