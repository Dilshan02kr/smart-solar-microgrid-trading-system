/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: ProsumerRegistrationService.cs
 * Component: Prosumer Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Validates Prosumer self-registration and creates a securely hashed pending account.
 */
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class ProsumerRegistrationService(
    IUserDetailsRepository userDetailsRepository,
    IPasswordHasher<UserDetails> passwordHasher) : IProsumerRegistrationService
{
    public async Task<ProsumerRegistrationResult> RegisterAsync(
        RegisterProsumerRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validates unique Prosumer details and persists a securely hashed pending account.
        var nic = request.Nic?.Trim();
        var firstName = request.FirstName?.Trim();
        var lastName = request.LastName?.Trim();
        var email = request.Email?.Trim().ToLowerInvariant();
        var phone = request.Phone?.Trim();

        if (string.IsNullOrWhiteSpace(nic) ||
            string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            return Invalid("All registration fields are required and cannot contain only whitespace.");
        }

        if (!new EmailAddressAttribute().IsValid(email))
        {
            return Invalid("Email must be a valid email address.");
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return Invalid("Password and confirmPassword must match.");
        }

        if (await userDetailsRepository.GetByNicAsync(nic, cancellationToken) is not null)
        {
            return new ProsumerRegistrationResult(ProsumerRegistrationStatus.NicAlreadyExists);
        }

        if (await userDetailsRepository.GetByEmailAsync(email, cancellationToken) is not null)
        {
            return new ProsumerRegistrationResult(ProsumerRegistrationStatus.EmailAlreadyExists);
        }

        var prosumer = new UserDetails
        {
            Nic = nic,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            PasswordHash = string.Empty,
            Role = UserRole.PROSUMER,
            AccountStatus = AccountStatus.PENDING
        };

        prosumer.PasswordHash = passwordHasher.HashPassword(prosumer, request.Password);

        try
        {
            await userDetailsRepository.CreateAsync(prosumer, cancellationToken);
        }
        catch (DuplicateUserDetailsException exception)
        {
            return exception.Field switch
            {
                DuplicateUserField.Nic =>
                    new ProsumerRegistrationResult(ProsumerRegistrationStatus.NicAlreadyExists),
                DuplicateUserField.Email =>
                    new ProsumerRegistrationResult(ProsumerRegistrationStatus.EmailAlreadyExists),
                _ => new ProsumerRegistrationResult(
                    ProsumerRegistrationStatus.Conflict,
                    Message: "A user with the supplied unique details already exists.")
            };
        }

        var response = new ProsumerRegistrationResponse(
            prosumer.Id.ToString(),
            prosumer.Nic,
            prosumer.FirstName,
            prosumer.LastName,
            prosumer.Email,
            prosumer.Phone,
            prosumer.Role.ToString(),
            prosumer.AccountStatus.ToString());

        return new ProsumerRegistrationResult(ProsumerRegistrationStatus.Created, response);
    }

    // Creates a validation-failure result with a safe client-facing message.
    private static ProsumerRegistrationResult Invalid(string message) =>
        new(ProsumerRegistrationStatus.Invalid, Message: message);
}
