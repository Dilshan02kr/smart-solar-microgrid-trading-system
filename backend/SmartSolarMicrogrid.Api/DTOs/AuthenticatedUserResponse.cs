/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: AuthenticatedUserResponse.cs
 * Component: Authentication and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Returns the authenticated user's safe identity, role, status, and applicable assignment data.
 */
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record AuthenticatedUserResponse(
    string UserId,
    string FirstName,
    string LastName,
    string Role,
    string AccountStatus,
    string? Nic = null,
    string? AssignedMicrogridNodeId = null);
