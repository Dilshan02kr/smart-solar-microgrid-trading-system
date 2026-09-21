using Microsoft.AspNetCore.Identity;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class AuthenticationService(
    IUserDetailsRepository userDetailsRepository,
    IPasswordHasher<UserDetails> passwordHasher,
    IJwtTokenService jwtTokenService) : IAuthenticationService
{
    public async Task<AuthenticationResult> WebLoginAsync(
        WebLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return InvalidCredentials();
        }

        var user = await userDetailsRepository.GetByEmailAsync(
            request.Email.Trim().ToLowerInvariant(),
            cancellationToken);

        if (user is null ||
            user.Role is not (UserRole.BACKOFFICE or UserRole.GRID_OPERATOR) ||
            !PasswordMatches(user, request.Password))
        {
            return InvalidCredentials();
        }

        return AuthenticateActiveUser(user);
    }

    public async Task<AuthenticationResult> ProsumerLoginAsync(
        ProsumerLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nic) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return InvalidCredentials();
        }

        var user = await userDetailsRepository.GetByNicAsync(
            request.Nic.Trim(),
            cancellationToken);

        if (user is null ||
            user.Role != UserRole.PROSUMER ||
            !PasswordMatches(user, request.Password))
        {
            return InvalidCredentials();
        }

        return AuthenticateActiveUser(user);
    }

    public async Task<AuthenticatedUserResponse?> GetCurrentUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userDetailsRepository.GetByIdAsync(userId, cancellationToken);
        return user?.AccountStatus == AccountStatus.ACTIVE ? MapUser(user) : null;
    }

    private AuthenticationResult AuthenticateActiveUser(UserDetails user)
    {
        if (user.AccountStatus == AccountStatus.PENDING)
        {
            return new AuthenticationResult(AuthenticationStatus.AccountPending);
        }

        if (user.AccountStatus == AccountStatus.DEACTIVATED)
        {
            return new AuthenticationResult(AuthenticationStatus.AccountDeactivated);
        }

        if (user.AccountStatus != AccountStatus.ACTIVE)
        {
            return InvalidCredentials();
        }

        var token = jwtTokenService.CreateToken(user);
        return new AuthenticationResult(
            AuthenticationStatus.Success,
            new AuthenticationResponse(token.Token, token.ExpiresAtUtc, MapUser(user)));
    }

    private bool PasswordMatches(UserDetails user, string password) =>
        passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) !=
        PasswordVerificationResult.Failed;

    private static AuthenticatedUserResponse MapUser(UserDetails user) =>
        new(
            user.Id.ToString(),
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.AccountStatus.ToString(),
            user.Role == UserRole.PROSUMER ? user.Nic : null,
            user.Role == UserRole.GRID_OPERATOR ? user.AssignedMicrogridNodeId?.ToString() : null);

    private static AuthenticationResult InvalidCredentials() =>
        new(AuthenticationStatus.InvalidCredentials);
}
