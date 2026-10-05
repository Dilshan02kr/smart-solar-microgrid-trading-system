/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: ActiveAccountJwtBearerEvents.cs
 * Component: Authentication and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Enforces current database role and ACTIVE account status for every validated JWT.
 */
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartSolarMicrogrid.Api.Common;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Security;

public sealed class ActiveAccountJwtBearerEvents(
    IUserDetailsRepository userDetailsRepository,
    IMemoryCache memoryCache,
    ILogger<ActiveAccountJwtBearerEvents> logger) : JwtBearerEvents
{
    private const string ErrorCodeItem = "AuthenticationErrorCode";
    private const string ErrorMessageItem = "AuthenticationErrorMessage";
    private static readonly TimeSpan AccountCacheDuration = TimeSpan.FromSeconds(30);

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var cancellationToken = context.HttpContext.RequestAborted;
        if (cancellationToken.IsCancellationRequested)
        {
            context.Fail("Request was cancelled.");
            return;
        }

        var userId = context.Principal?.FindFirst("userId")?.Value;
        var tokenRole = context.Principal?.FindFirst("role")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            SetFailure(context, AuthenticationErrorCodes.AuthenticationRequired,
                "A valid authentication token is required.");
            return;
        }

        var cacheKey = $"active_account_{userId}";
        if (memoryCache.TryGetValue(cacheKey, out (string Role, AccountStatus AccountStatus) cached))
        {
            ValidateAccount(context, tokenRole, cached.Role, cached.AccountStatus);
            return;
        }

        UserDetails? user;
        try
        {
            user = await userDetailsRepository.GetByIdAsync(userId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            context.Fail("Request was cancelled.");
            return;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to verify account status for user {UserId} due to database error.", userId);
            SetFailure(context, AuthenticationErrorCodes.AuthenticationRequired,
                "Unable to verify account status. Please try again.");
            return;
        }

        if (user is null)
        {
            SetFailure(context, AuthenticationErrorCodes.AuthenticationRequired,
                "A valid authentication token is required.");
            return;
        }

        var roleString = user.Role.ToString();
        memoryCache.Set(cacheKey, (roleString, user.AccountStatus), AccountCacheDuration);
        ValidateAccount(context, tokenRole, roleString, user.AccountStatus);
    }

    private static void ValidateAccount(
        TokenValidatedContext context,
        string? tokenRole,
        string actualRole,
        AccountStatus accountStatus)
    {
        if (!string.Equals(tokenRole, actualRole, StringComparison.Ordinal))
        {
            SetFailure(context, AuthenticationErrorCodes.AuthenticationRequired,
                "A valid authentication token is required.");
            return;
        }

        if (accountStatus == AccountStatus.PENDING)
        {
            SetFailure(context, AuthenticationErrorCodes.AccountPending,
                "The account is pending activation.");
            return;
        }

        if (accountStatus != AccountStatus.ACTIVE)
        {
            SetFailure(context, AuthenticationErrorCodes.AccountDeactivated,
                "The account is deactivated.");
        }
    }

    public override async Task Challenge(JwtBearerChallengeContext context)
    {
        if (context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            return;
        }

        // Returns a structured 401 response for authentication failures.
        context.HandleResponse();

        var code = context.HttpContext.Items[ErrorCodeItem] as string
            ?? AuthenticationErrorCodes.AuthenticationRequired;
        var message = context.HttpContext.Items[ErrorMessageItem] as string
            ?? "A valid authentication token is required.";

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        try
        {
            await context.Response.WriteAsJsonAsync(
                new ApiErrorResponse(code, message),
                context.HttpContext.RequestAborted);
        }
        catch (OperationCanceledException)
        {
            // Client aborted connection.
        }
    }

    public override async Task Forbidden(ForbiddenContext context)
    {
        if (context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            return;
        }

        // Returns a structured 403 response for authenticated role failures.
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        try
        {
            await context.Response.WriteAsJsonAsync(
                new ApiErrorResponse(
                    AuthenticationErrorCodes.AccessDenied,
                    "The authenticated account does not have permission to access this resource."),
                context.HttpContext.RequestAborted);
        }
        catch (OperationCanceledException)
        {
            // Client aborted connection.
        }
    }

    private static void SetFailure(
        TokenValidatedContext context,
        string code,
        string message)
    {
        // Records the safe error details used by the later authentication challenge.
        context.HttpContext.Items[ErrorCodeItem] = code;
        context.HttpContext.Items[ErrorMessageItem] = message;
        context.Fail(code);
    }
}
