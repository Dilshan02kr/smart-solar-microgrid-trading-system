/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: UserErrorCodes.cs
 * Component: Web User and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines stable API error codes for Web-user and self-profile operations.
 */
namespace SmartSolarMicrogrid.Api.Common;

public static class UserErrorCodes
{
    public const string InvalidUserId = "INVALID_USER_ID";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string InvalidUserRole = "INVALID_USER_ROLE";
    public const string InvalidAccountState = "INVALID_ACCOUNT_STATE";
    public const string InvalidStationId = "INVALID_STATION_ID";
    public const string StationNotFound = "STATION_NOT_FOUND";
    public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
    public const string OperatorStationRequired = "OPERATOR_STATION_REQUIRED";
    public const string ValidationError = "VALIDATION_ERROR";
}
