/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: CreateWebUserRequest.cs
 * Component: Web User and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines client-controlled fields for Backoffice creation of Web users.
 */
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class CreateWebUserRequest
{
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

    [Required]
    public string? Role { get; init; }

    public string? AssignedMicrogridNodeId { get; init; }
}
