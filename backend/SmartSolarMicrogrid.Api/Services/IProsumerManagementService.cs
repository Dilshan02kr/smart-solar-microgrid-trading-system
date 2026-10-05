/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IProsumerManagementService.cs
 * Component: Prosumer Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines Backoffice lifecycle and authenticated self-profile operations for Prosumers.
 */
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IProsumerManagementService
{
    // Returns every Prosumer for Backoffice administration.
    Task<IReadOnlyList<ProsumerResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // Returns Prosumers awaiting Backoffice activation.
    Task<IReadOnlyList<ProsumerResponse>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    // Validates and retrieves one Prosumer for Backoffice.
    Task<ProsumerManagementResult> GetByIdAsync(
        string prosumerId,
        CancellationToken cancellationToken = default);

    // Transitions a pending Prosumer to active.
    Task<ProsumerManagementResult> ActivateAsync(
        string prosumerId,
        CancellationToken cancellationToken = default);

    // Transitions a deactivated Prosumer back to active.
    Task<ProsumerManagementResult> ReactivateAsync(
        string prosumerId,
        CancellationToken cancellationToken = default);

    // Returns the authenticated active Prosumer's profile.
    Task<ProsumerManagementResult> GetMeAsync(
        string authenticatedUserId,
        CancellationToken cancellationToken = default);

    // Updates only the authenticated Prosumer's editable profile fields.
    Task<ProsumerManagementResult> UpdateMeAsync(
        string authenticatedUserId,
        UpdateProsumerProfileRequest request,
        CancellationToken cancellationToken = default);

    // Atomically deactivates the authenticated active Prosumer.
    Task<ProsumerManagementResult> DeactivateMeAsync(
        string authenticatedUserId,
        CancellationToken cancellationToken = default);
}

public enum ProsumerManagementStatus
{
    Success,
    InvalidId,
    NotFound,
    InvalidAccountState,
    EmailAlreadyExists,
    ValidationError
}

public sealed record ProsumerManagementResult(
    ProsumerManagementStatus Status,
    ProsumerResponse? Prosumer = null,
    string? ErrorMessage = null);
