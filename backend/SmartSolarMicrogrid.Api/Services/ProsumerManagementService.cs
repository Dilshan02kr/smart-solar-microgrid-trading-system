using MongoDB.Bson;
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
        var users = await userDetailsRepository.GetByRoleAsync(
            UserRole.PROSUMER,
            cancellationToken);

        return users.Select(MapProsumer).ToList();
    }

    public async Task<IReadOnlyList<ProsumerResponse>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
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
        if (!ObjectId.TryParse(prosumerId, out _))
        {
            return new ProsumerManagementResult(ProsumerManagementStatus.InvalidId);
        }

        var user = await userDetailsRepository.GetByIdAsync(prosumerId, cancellationToken);
        return user?.Role == UserRole.PROSUMER
            ? Success(user)
            : new ProsumerManagementResult(ProsumerManagementStatus.NotFound);
    }

    public Task<ProsumerManagementResult> ActivateAsync(
        string prosumerId,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(
            prosumerId,
            AccountStatus.PENDING,
            cancellationToken);

    public Task<ProsumerManagementResult> ReactivateAsync(
        string prosumerId,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(
            prosumerId,
            AccountStatus.DEACTIVATED,
            cancellationToken);

    private async Task<ProsumerManagementResult> TransitionAsync(
        string prosumerId,
        AccountStatus expectedStatus,
        CancellationToken cancellationToken)
    {
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

    private static ProsumerManagementResult Success(UserDetails user) =>
        new(ProsumerManagementStatus.Success, MapProsumer(user));

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
