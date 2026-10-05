/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: ProsumerRegistrationResponse.cs
 * Component: Prosumer Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Returns safe account details after Prosumer self-registration.
 */
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record ProsumerRegistrationResponse(
    string UserId,
    string Nic,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role,
    string AccountStatus);
