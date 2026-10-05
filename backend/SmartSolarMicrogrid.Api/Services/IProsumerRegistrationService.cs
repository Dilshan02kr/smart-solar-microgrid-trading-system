/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IProsumerRegistrationService.cs
 * Component: Prosumer Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines public Prosumer self-registration with server-controlled initial state.
 */
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IProsumerRegistrationService
{
    // Validates and creates a pending Prosumer account.
    Task<ProsumerRegistrationResult> RegisterAsync(
        RegisterProsumerRequest request,
        CancellationToken cancellationToken = default);
}

public enum ProsumerRegistrationStatus
{
    Created,
    Invalid,
    NicAlreadyExists,
    EmailAlreadyExists,
    Conflict
}

public sealed record ProsumerRegistrationResult(
    ProsumerRegistrationStatus Status,
    ProsumerRegistrationResponse? Prosumer = null,
    string? Message = null);
