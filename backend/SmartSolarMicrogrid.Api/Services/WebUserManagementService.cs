/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: WebUserManagementService.cs
 * Component: Web User and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Implements validated Backoffice management of Backoffice and Grid Operator accounts.
 */
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class WebUserManagementService(
    IUserDetailsRepository userRepository,
    ISolarStationRepository stationRepository,
    IPasswordHasher<UserDetails> passwordHasher) : IWebUserManagementService
{
    public async Task<WebUserManagementResult> CreateAsync(
        CreateWebUserRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate client fields and create a server-controlled active Web-user account.
        var fields = ValidateProfile(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone);
        if (fields.Error is not null)
        {
            return Invalid(fields.Error);
        }

        if (string.IsNullOrWhiteSpace(request.Password) ||
            !string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return Invalid("Password and confirmPassword are required and must match.");
        }

        if (!Enum.TryParse<UserRole>(request.Role?.Trim(), false, out var role) ||
            role is not (UserRole.BACKOFFICE or UserRole.GRID_OPERATOR))
        {
            return new WebUserManagementResult(WebUserManagementStatus.InvalidUserRole);
        }

        var assignment = await ValidateAssignmentAsync(
            role,
            request.AssignedMicrogridNodeId,
            cancellationToken);
        if (assignment.Failure is not null)
        {
            return assignment.Failure;
        }

        if (await userRepository.GetByEmailAsync(fields.Email!, cancellationToken) is not null)
        {
            return new WebUserManagementResult(WebUserManagementStatus.EmailAlreadyExists);
        }

        var user = new UserDetails
        {
            FirstName = fields.FirstName!,
            LastName = fields.LastName!,
            Email = fields.Email!,
            Phone = fields.Phone!,
            PasswordHash = string.Empty,
            Role = role,
            AccountStatus = AccountStatus.ACTIVE,
            AssignedMicrogridNodeId = assignment.StationId
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        try
        {
            await userRepository.CreateAsync(user, cancellationToken);
        }
        catch (DuplicateUserDetailsException)
        {
            return new WebUserManagementResult(WebUserManagementStatus.EmailAlreadyExists);
        }

        return Success(user);
    }

    public async Task<IReadOnlyList<WebUserResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // Return safe DTOs for Backoffice and Grid Operator accounts only.
        var users = await userRepository.GetWebUsersAsync(cancellationToken);
        return users.Select(MapUser).ToList();
    }

    public async Task<WebUserManagementResult> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Retrieve a supported Web user after validating its public ObjectId string.
        if (!ObjectId.TryParse(userId, out _))
        {
            return new WebUserManagementResult(WebUserManagementStatus.InvalidUserId);
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        return IsWebRole(user?.Role) ? Success(user!) : NotFound();
    }

    public async Task<WebUserManagementResult> UpdateAsync(
        string userId,
        UpdateWebUserRequest request,
        CancellationToken cancellationToken = default)
    {
        // Update editable fields and validate any current Grid Operator station assignment.
        if (!ObjectId.TryParse(userId, out _))
        {
            return new WebUserManagementResult(WebUserManagementStatus.InvalidUserId);
        }

        var current = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (!IsWebRole(current?.Role))
        {
            return NotFound();
        }

        var fields = ValidateProfile(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone);
        if (fields.Error is not null)
        {
            return Invalid(fields.Error);
        }

        var assignment = await ValidateAssignmentAsync(
            current!.Role,
            request.AssignedMicrogridNodeId,
            cancellationToken);
        if (assignment.Failure is not null)
        {
            return assignment.Failure;
        }

        var emailOwner = await userRepository.GetByEmailAsync(fields.Email!, cancellationToken);
        if (emailOwner is not null && emailOwner.Id != current.Id)
        {
            return new WebUserManagementResult(WebUserManagementStatus.EmailAlreadyExists);
        }

        try
        {
            var updated = await userRepository.UpdateWebUserAsync(
                userId,
                current.Role,
                fields.FirstName!,
                fields.LastName!,
                fields.Email!,
                fields.Phone!,
                assignment.StationId,
                cancellationToken);
            return updated is not null ? Success(updated) : NotFound();
        }
        catch (DuplicateUserDetailsException)
        {
            return new WebUserManagementResult(WebUserManagementStatus.EmailAlreadyExists);
        }
    }

    public async Task<WebUserManagementResult> DeactivateAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Atomically deactivate an active Backoffice or Grid Operator account without deleting it.
        if (!ObjectId.TryParse(userId, out _))
        {
            return new WebUserManagementResult(WebUserManagementStatus.InvalidUserId);
        }

        var current = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (!IsWebRole(current?.Role))
        {
            return NotFound();
        }

        if (current!.AccountStatus != AccountStatus.ACTIVE)
        {
            return new WebUserManagementResult(WebUserManagementStatus.InvalidAccountState);
        }

        var updated = await userRepository.TryTransitionAccountStatusAsync(
            userId,
            current.Role,
            AccountStatus.ACTIVE,
            AccountStatus.DEACTIVATED,
            cancellationToken);
        return updated is not null
            ? Success(updated)
            : new WebUserManagementResult(WebUserManagementStatus.InvalidAccountState);
    }

    private async Task<(ObjectId? StationId, WebUserManagementResult? Failure)> ValidateAssignmentAsync(
        UserRole role,
        string? stationId,
        CancellationToken cancellationToken)
    {
        // Enforce role-specific station assignment and resolve it to a typed ObjectId.
        if (role == UserRole.BACKOFFICE)
        {
            return string.IsNullOrWhiteSpace(stationId)
                ? (null, null)
                : (null, Invalid("Backoffice accounts cannot have an assigned station."));
        }

        if (string.IsNullOrWhiteSpace(stationId))
        {
            return (null, new WebUserManagementResult(
                WebUserManagementStatus.OperatorStationRequired));
        }

        if (!ObjectId.TryParse(stationId.Trim(), out var stationObjectId))
        {
            return (null, new WebUserManagementResult(WebUserManagementStatus.InvalidStationId));
        }

        var station = await stationRepository.GetByIdAsync(stationId.Trim(), cancellationToken);
        return station is null
            ? (null, new WebUserManagementResult(WebUserManagementStatus.StationNotFound))
            : (stationObjectId, null);
    }

    private static (string? FirstName, string? LastName, string? Email, string? Phone, string? Error)
        ValidateProfile(string? firstName, string? lastName, string? email, string? phone)
    {
        // Normalize and validate the shared editable identity fields.
        var normalizedFirstName = firstName?.Trim();
        var normalizedLastName = lastName?.Trim();
        var normalizedEmail = email?.Trim().ToLowerInvariant();
        var normalizedPhone = phone?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedFirstName) ||
            string.IsNullOrWhiteSpace(normalizedLastName) ||
            string.IsNullOrWhiteSpace(normalizedEmail) ||
            string.IsNullOrWhiteSpace(normalizedPhone))
        {
            return (null, null, null, null, "All profile fields are required.");
        }

        if (!new EmailAddressAttribute().IsValid(normalizedEmail))
        {
            return (null, null, null, null, "Email must be a valid email address.");
        }

        return (normalizedFirstName, normalizedLastName, normalizedEmail, normalizedPhone, null);
    }

    private static bool IsWebRole(UserRole? role)
    {
        // Identify roles managed through the Backoffice Web-user API.
        return role is UserRole.BACKOFFICE or UserRole.GRID_OPERATOR;
    }

    private static WebUserManagementResult Success(UserDetails user)
    {
        // Map a successful persistence result to the safe response contract.
        return new WebUserManagementResult(WebUserManagementStatus.Success, MapUser(user));
    }

    private static WebUserManagementResult NotFound()
    {
        // Return the uniform not-found outcome used for missing or unsupported roles.
        return new WebUserManagementResult(WebUserManagementStatus.UserNotFound);
    }

    private static WebUserManagementResult Invalid(string message)
    {
        // Return a validation result with a safe client-facing message.
        return new WebUserManagementResult(
            WebUserManagementStatus.ValidationError,
            ErrorMessage: message);
    }

    private static WebUserResponse MapUser(UserDetails user)
    {
        // Expose only non-sensitive Web-user fields.
        return new WebUserResponse(
            user.Id.ToString(),
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            user.Role.ToString(),
            user.AccountStatus.ToString(),
            user.Role == UserRole.GRID_OPERATOR
                ? user.AssignedMicrogridNodeId?.ToString()
                : null,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
