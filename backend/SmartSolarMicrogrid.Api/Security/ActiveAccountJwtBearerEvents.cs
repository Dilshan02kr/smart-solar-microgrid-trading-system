using Microsoft.AspNetCore.Authentication.JwtBearer;
using SmartSolarMicrogrid.Api.Common;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Security;

public sealed class ActiveAccountJwtBearerEvents(
    IUserDetailsRepository userDetailsRepository) : JwtBearerEvents
{
    private const string ErrorCodeItem = "AuthenticationErrorCode";
    private const string ErrorMessageItem = "AuthenticationErrorMessage";

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var userId = context.Principal?.FindFirst("userId")?.Value;
        var tokenRole = context.Principal?.FindFirst("role")?.Value;
        var user = string.IsNullOrWhiteSpace(userId)
            ? null
            : await userDetailsRepository.GetByIdAsync(userId, context.HttpContext.RequestAborted);

        if (user is null || !string.Equals(tokenRole, user.Role.ToString(), StringComparison.Ordinal))
        {
            SetFailure(context, AuthenticationErrorCodes.AuthenticationRequired,
                "A valid authentication token is required.");
            return;
        }

        if (user.AccountStatus == AccountStatus.PENDING)
        {
            SetFailure(context, AuthenticationErrorCodes.AccountPending,
                "The account is pending activation.");
            return;
        }

        if (user.AccountStatus != AccountStatus.ACTIVE)
        {
            SetFailure(context, AuthenticationErrorCodes.AccountDeactivated,
                "The account is deactivated.");
        }
    }

    public override async Task Challenge(JwtBearerChallengeContext context)
    {
        context.HandleResponse();

        var code = context.HttpContext.Items[ErrorCodeItem] as string
            ?? AuthenticationErrorCodes.AuthenticationRequired;
        var message = context.HttpContext.Items[ErrorMessageItem] as string
            ?? "A valid authentication token is required.";

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new ApiErrorResponse(code, message));
    }

    public override async Task Forbidden(ForbiddenContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new ApiErrorResponse(
            AuthenticationErrorCodes.AccessDenied,
            "The authenticated account does not have permission to access this resource."));
    }

    private static void SetFailure(
        TokenValidatedContext context,
        string code,
        string message)
    {
        context.HttpContext.Items[ErrorCodeItem] = code;
        context.HttpContext.Items[ErrorMessageItem] = message;
        context.Fail(code);
    }
}
