using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Repositories;

public interface IUserDetailsRepository
{
    Task<UserDetails?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<UserDetails?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<UserDetails?> GetByNicAsync(string nic, CancellationToken cancellationToken = default);

    Task<bool> ExistsByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDetails>> GetByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDetails>> GetByRoleAndStatusAsync(
        UserRole role,
        AccountStatus accountStatus,
        CancellationToken cancellationToken = default);

    Task<UserDetails?> TryTransitionAccountStatusAsync(
        string id,
        UserRole role,
        AccountStatus expectedStatus,
        AccountStatus newStatus,
        CancellationToken cancellationToken = default);

    Task CreateAsync(UserDetails user, CancellationToken cancellationToken = default);

    Task EnsureIndexesAsync(CancellationToken cancellationToken = default);
}
