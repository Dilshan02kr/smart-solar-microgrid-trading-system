using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class ProsumerLoginRequest
{
    [Required]
    public string? Nic { get; init; }

    [Required]
    public string? Password { get; init; }
}
