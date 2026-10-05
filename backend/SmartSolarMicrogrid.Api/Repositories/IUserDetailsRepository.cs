/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: IUserDetailsRepository.cs
 * Component: Web User and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines MongoDB persistence operations for authentication and account management.
 */
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Repositories;

public interface IUserDetailsRepository
{
    // Retrieves one user by a validated public identifier.
    Task<UserDetails?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    // Retrieves the account owning a normalized email address.
    Task<UserDetails?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    // Retrieves the Prosumer account owning a NIC.
    Task<UserDetails?> GetByNicAsync(string nic, CancellationToken cancellationToken = default);

    // Determines whether any account exists for a role.
    Task<bool> ExistsByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

    // Returns every account for a role.
    Task<IReadOnlyList<UserDetails>> GetByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default);

    // Returns accounts matching a role and lifecycle status.
    Task<IReadOnlyList<UserDetails>> GetByRoleAndStatusAsync(
        UserRole role,
        AccountStatus accountStatus,
        CancellationToken cancellationToken = default);

    // Returns only Backoffice and Grid Operator accounts.
    Task<IReadOnlyList<UserDetails>> GetWebUsersAsync(
        CancellationToken cancellationToken = default);

    // Atomically updates editable Web-user fields and operator assignment.
    Task<UserDetails?> UpdateWebUserAsync(
        string id,
        UserRole expectedRole,
        string firstName,
        string lastName,
        string email,
        string phone,
        MongoDB.Bson.ObjectId? assignedMicrogridNodeId,
        CancellationToken cancellationToken = default);

    // Atomically updates the authenticated active Prosumer's profile fields.
    Task<UserDetails?> UpdateProsumerProfileAsync(
        string id,
        string firstName,
        string lastName,
        string email,
        string phone,
        CancellationToken cancellationToken = default);

    // Atomically performs an expected account-status transition for one role.
    Task<UserDetails?> TryTransitionAccountStatusAsync(
        string id,
        UserRole role,
        AccountStatus expectedStatus,
        AccountStatus newStatus,
        CancellationToken cancellationToken = default);

    // Inserts a normalized server-controlled user account.
    Task CreateAsync(UserDetails user, CancellationToken cancellationToken = default);

    // Creates the unique indexes required by account contracts.
    Task EnsureIndexesAsync(CancellationToken cancellationToken = default);
}
