/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: UpdateProsumerProfileRequest.cs
 * Component: Prosumer Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines the limited fields a Prosumer may change on their own profile.
 */
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class UpdateProsumerProfileRequest
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
}
