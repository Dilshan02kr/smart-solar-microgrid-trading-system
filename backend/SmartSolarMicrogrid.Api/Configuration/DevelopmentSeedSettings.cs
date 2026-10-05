/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: DevelopmentSeedSettings.cs
 * Component: Development Integration Configuration
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Represents configuration options for optional development-only data seeding.
 */
namespace SmartSolarMicrogrid.Api.Configuration;

public sealed class DevelopmentSeedSettings
{
    public const string SectionName = "DevelopmentSeed";

    public bool Enabled { get; init; }

    public string OperatorPassword { get; init; } = string.Empty;

    public string ProsumerPassword { get; init; } = string.Empty;
}
