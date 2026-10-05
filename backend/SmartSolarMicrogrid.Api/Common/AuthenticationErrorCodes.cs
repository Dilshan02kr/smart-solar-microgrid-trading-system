/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: AuthenticationErrorCodes.cs
 * Component: Authentication and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines stable error codes for authentication and authorization failures.
 */
namespace SmartSolarMicrogrid.Api.Common;

public static class AuthenticationErrorCodes
{
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string AccountPending = "ACCOUNT_PENDING";
    public const string AccountDeactivated = "ACCOUNT_DEACTIVATED";
    public const string AuthenticationRequired = "AUTHENTICATION_REQUIRED";
    public const string AccessDenied = "ACCESS_DENIED";
}
