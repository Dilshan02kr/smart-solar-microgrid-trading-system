/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: ProsumerManagementService.cs
 * Component: Prosumer Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Implements Backoffice lifecycle and authenticated self-profile operations for Prosumers.
 */
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class ProsumerManagementService(
    IUserDetailsRepository userDetailsRepository) : IProsumerManagementService
{
    public async Task<IReadOnlyList<ProsumerResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // Returns safe DTOs for every Prosumer account.
        var users = await userDetailsRepository.GetByRoleAsync(
            UserRole.PROSUMER,
            cancellationToken);

        return users.Select(MapProsumer).ToList();
    }

    public async Task<IReadOnlyList<ProsumerResponse>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        // Returns safe DTOs for Prosumers awaiting activation.
        var users = await userDetailsRepository.GetByRoleAndStatusAsync(
            UserRole.PROSUMER,
            AccountStatus.PENDING,
            cancellationToken);

        return users.Select(MapProsumer).ToList();
    }

    public async Task<ProsumerManagementResult> GetByIdAsync(
        string prosumerId,
        CancellationToken cancellationToken = default)
    {
        // Retrieves one Prosumer after validating its ObjectId string and role.
        if (!ObjectId.TryParse(prosumerId, out _))
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.InvalidId);
        }

        var user = await userDetailsRepository.GetByIdAsync(prosumerId, cancellationToken);
        return user?.Role == UserRole.PROSUMER
            ? Success(user)
            : new ProsumerManagementResult(ProsumerManagementStatus.NotFound);
    }

    // Transitions a pending Prosumer to active.
    public Task<ProsumerManagementResult> ActivateAsync(
        string prosumerId,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(
            prosumerId,
            AccountStatus.PENDING,
            cancellationToken);

    // Transitions a deactivated Prosumer back to active.
    public Task<ProsumerManagementResult> ReactivateAsync(
        string prosumerId,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(
            prosumerId,
            AccountStatus.DEACTIVATED,
            cancellationToken);

    public async Task<ProsumerManagementResult> GetMeAsync(
        string authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        // Return the authenticated active Prosumer's own safe profile.
        if (!ObjectId.TryParse(authenticatedUserId, out _))
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.InvalidId);
        }

        var user = await userDetailsRepository.GetByIdAsync(
            authenticatedUserId,
            cancellationToken);
        return user?.Role == UserRole.PROSUMER && user.AccountStatus == AccountStatus.ACTIVE
            ? Success(user)
            : new ProsumerManagementResult(ProsumerManagementStatus.NotFound);
    }

    public async Task<ProsumerManagementResult> UpdateMeAsync(
        string authenticatedUserId,
        UpdateProsumerProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        // Update only the authenticated Prosumer's editable profile fields.
        if (!ObjectId.TryParse(authenticatedUserId, out _))
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.InvalidId);
        }

        var current = await userDetailsRepository.GetByIdAsync(
            authenticatedUserId,
            cancellationToken);
        if (current?.Role != UserRole.PROSUMER || current.AccountStatus != AccountStatus.ACTIVE)
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.NotFound);
        }

        var firstName = request.FirstName?.Trim();
        var lastName = request.LastName?.Trim();
        var email = request.Email?.Trim().ToLowerInvariant();
        var phone = request.Phone?.Trim();
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phone) ||
            !new EmailAddressAttribute().IsValid(email))
        {
            return new ProsumerManagementResult(
                ProsumerManagementStatus.ValidationError,
                ErrorMessage: "Valid firstName, lastName, email, and phone values are required.");
        }

        var emailOwner = await userDetailsRepository.GetByEmailAsync(email, cancellationToken);
        if (emailOwner is not null && emailOwner.Id != current.Id)
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.EmailAlreadyExists);
        }

        try
        {
            var updated = await userDetailsRepository.UpdateProsumerProfileAsync(
                authenticatedUserId,
                firstName,
                lastName,
                email,
                phone,
                cancellationToken);
            return updated is not null
                ? Success(updated)
                : new ProsumerManagementResult(ProsumerManagementStatus.InvalidAccountState);
        }
        catch (DuplicateUserDetailsException)
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.EmailAlreadyExists);
        }
    }

    public Task<ProsumerManagementResult> DeactivateMeAsync(
        string authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        // Deactivate only the authenticated active Prosumer using the existing atomic transition.
        return DeactivateSelfAsync(authenticatedUserId, cancellationToken);
    }

    private async Task<ProsumerManagementResult> DeactivateSelfAsync(
        string authenticatedUserId,
        CancellationToken cancellationToken)
    {
        // Validate ownership state and perform ACTIVE to DEACTIVATED atomically.
        if (!ObjectId.TryParse(authenticatedUserId, out _))
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.InvalidId);
        }

        var current = await userDetailsRepository.GetByIdAsync(
            authenticatedUserId,
            cancellationToken);
        if (current?.Role != UserRole.PROSUMER)
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.NotFound);
        }

        if (current.AccountStatus != AccountStatus.ACTIVE)
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.InvalidAccountState);
        }

        var updated = await userDetailsRepository.TryTransitionAccountStatusAsync(
            authenticatedUserId,
            UserRole.PROSUMER,
            AccountStatus.ACTIVE,
            AccountStatus.DEACTIVATED,
            cancellationToken);
        return updated is not null
            ? Success(updated)
            : new ProsumerManagementResult(ProsumerManagementStatus.InvalidAccountState);
    }

    private async Task<ProsumerManagementResult> TransitionAsync(
        string prosumerId,
        AccountStatus expectedStatus,
        CancellationToken cancellationToken)
    {
        // Validates and atomically applies an expected Prosumer account-state transition.
        if (!ObjectId.TryParse(prosumerId, out _))
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.InvalidId);
        }

        var currentUser = await userDetailsRepository.GetByIdAsync(prosumerId, cancellationToken);
        if (currentUser?.Role != UserRole.PROSUMER)
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.NotFound);
        }

        if (currentUser.AccountStatus != expectedStatus)
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.InvalidAccountState);
        }

        var updatedUser = await userDetailsRepository.TryTransitionAccountStatusAsync(
            prosumerId,
            UserRole.PROSUMER,
            expectedStatus,
            AccountStatus.ACTIVE,
            cancellationToken);

        if (updatedUser is not null)
        {
            return Success(updatedUser);
        }

        // If another request changed the record after the first read, report its current state.
        currentUser = await userDetailsRepository.GetByIdAsync(prosumerId, cancellationToken);
        return currentUser?.Role == UserRole.PROSUMER
            ? new ProsumerManagementResult(ProsumerManagementStatus.InvalidAccountState)
            : new ProsumerManagementResult(ProsumerManagementStatus.NotFound);
    }

    // Maps a user record to a successful safe Prosumer result.
    private static ProsumerManagementResult Success(UserDetails user) =>
        new(ProsumerManagementStatus.Success, MapProsumer(user));

    // Maps persisted Prosumer fields to the public profile response.
    private static ProsumerResponse MapProsumer(UserDetails user) =>
        new(
            user.Id.ToString(),
            user.Nic ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            user.AccountStatus.ToString(),
            user.CreatedAt,
            user.UpdatedAt);
}
