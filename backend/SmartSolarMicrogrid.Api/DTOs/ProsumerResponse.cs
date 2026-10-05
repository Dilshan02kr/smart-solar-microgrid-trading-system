/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: ProsumerResponse.cs
 * Component: Prosumer Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Exposes safe Prosumer profile and account-lifecycle information.
 */
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record ProsumerResponse(
    string UserId,
    string Nic,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string AccountStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt);
