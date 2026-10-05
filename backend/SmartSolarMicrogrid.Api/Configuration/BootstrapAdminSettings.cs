/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: BootstrapAdminSettings.cs
 * Component: Authentication Bootstrap Configuration
 *
 * Component Owners:
 * - WMDD Karunarathna (IT23145320) — initial Backoffice account configuration
 * - Kulunu Kasthuri Arachchi (IT23375628) — startup integration configuration
 *
 * Purpose:
 * Defines configuration used to provision the initial Backoffice account at application startup.
 */
namespace SmartSolarMicrogrid.Api.Configuration;

public sealed class BootstrapAdminSettings
{
    public const string SectionName = "BootstrapAdmin";

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
