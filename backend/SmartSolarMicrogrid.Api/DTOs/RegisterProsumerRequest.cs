/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: RegisterProsumerRequest.cs
 * Component: Prosumer Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines validated client fields for Prosumer self-registration.
 */
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class RegisterProsumerRequest
{
    [Required]
    public string? Nic { get; init; }

    [Required]
    public string? FirstName { get; init; }

    [Required]
    public string? LastName { get; init; }

    [Required]
    [EmailAddress]
    public string? Email { get; init; }

    [Required]
    public string? Phone { get; init; }

    [Required]
    public string? Password { get; init; }

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmPassword must match.")]
    public string? ConfirmPassword { get; init; }
}
