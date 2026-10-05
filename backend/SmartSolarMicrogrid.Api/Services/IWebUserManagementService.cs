/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IWebUserManagementService.cs
 * Component: Web User and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines Backoffice operations for managing Backoffice and Grid Operator accounts.
 */
using SmartSolarMicrogrid.Api.DTOs;

namespace SmartSolarMicrogrid.Api.Services;

public interface IWebUserManagementService
{
    // Create an active Backoffice or assigned Grid Operator account.
    Task<WebUserManagementResult> CreateAsync(
        CreateWebUserRequest request,
        CancellationToken cancellationToken = default);

    // List accounts supported by Web-user administration.
    Task<IReadOnlyList<WebUserResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // Retrieve one supported Web-user account.
    Task<WebUserManagementResult> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    // Update only editable fields while preserving role and lifecycle state.
    Task<WebUserManagementResult> UpdateAsync(
        string userId,
        UpdateWebUserRequest request,
        CancellationToken cancellationToken = default);

    // Atomically deactivate an active supported Web-user account.
    Task<WebUserManagementResult> DeactivateAsync(
        string userId,
        CancellationToken cancellationToken = default);
}

public enum WebUserManagementStatus
{
    Success,
    InvalidUserId,
    UserNotFound,
    InvalidUserRole,
    InvalidAccountState,
    InvalidStationId,
    StationNotFound,
    EmailAlreadyExists,
    OperatorStationRequired,
    ValidationError
}

public sealed record WebUserManagementResult(
    WebUserManagementStatus Status,
    WebUserResponse? User = null,
    string? ErrorMessage = null);
