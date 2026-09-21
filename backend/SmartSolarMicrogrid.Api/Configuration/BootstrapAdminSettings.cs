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
