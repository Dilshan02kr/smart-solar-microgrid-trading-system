// Creates the configured initial Backoffice account when none exists.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SmartSolarMicrogrid.Api.Configuration;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class BackofficeBootstrapInitializer(
    IUserDetailsRepository userDetailsRepository,
    IPasswordHasher<UserDetails> passwordHasher,
    IOptions<BootstrapAdminSettings> bootstrapOptions,
    ILogger<BackofficeBootstrapInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Creates the initial active Backoffice user only when no Backoffice account exists.
        if (await userDetailsRepository.ExistsByRoleAsync(UserRole.BACKOFFICE, cancellationToken))
        {
            logger.LogInformation("Backoffice account already exists; bootstrap skipped.");
            return;
        }

        var settings = bootstrapOptions.Value;
        ValidateSettings(settings);

        var backoffice = new UserDetails
        {
            FirstName = settings.FirstName.Trim(),
            LastName = settings.LastName.Trim(),
            Email = settings.Email.Trim().ToLowerInvariant(),
            Phone = settings.Phone.Trim(),
            PasswordHash = string.Empty,
            Role = UserRole.BACKOFFICE,
            AccountStatus = AccountStatus.ACTIVE,
            Nic = null,
            AssignedMicrogridNodeId = null
        };

        backoffice.PasswordHash = passwordHasher.HashPassword(backoffice, settings.Password);

        try
        {
            await userDetailsRepository.CreateAsync(backoffice, cancellationToken);
            logger.LogInformation("Initial Backoffice account created.");
        }
        catch (DuplicateUserDetailsException exception)
        {
            // Another application instance may have completed bootstrap after the first check.
            if (await userDetailsRepository.ExistsByRoleAsync(UserRole.BACKOFFICE, cancellationToken))
            {
                logger.LogInformation("Backoffice account already exists; bootstrap skipped.");
                return;
            }

            throw new InvalidOperationException(
                "Initial Backoffice bootstrap failed because its configured email is already in use.",
                exception);
        }
    }

    // Completes immediately because bootstrap initialization owns no shutdown resources.
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void ValidateSettings(BootstrapAdminSettings settings)
    {
        // Ensures all required bootstrap values are present and the email is valid.
        if (string.IsNullOrWhiteSpace(settings.FirstName) ||
            string.IsNullOrWhiteSpace(settings.LastName) ||
            string.IsNullOrWhiteSpace(settings.Email) ||
            string.IsNullOrWhiteSpace(settings.Phone) ||
            string.IsNullOrWhiteSpace(settings.Password))
        {
            throw new InvalidOperationException(
                "No Backoffice account exists. Configure all BootstrapAdmin values using a secure configuration provider such as .NET User Secrets.");
        }

        if (!new EmailAddressAttribute().IsValid(settings.Email.Trim()))
        {
            throw new InvalidOperationException(
                "No Backoffice account exists. BootstrapAdmin:Email must be a valid email address.");
        }
    }
}
