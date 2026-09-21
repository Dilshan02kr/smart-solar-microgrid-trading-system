using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class WebLoginRequest
{
    [Required]
    public string? Email { get; init; }

    [Required]
    public string? Password { get; init; }
}
