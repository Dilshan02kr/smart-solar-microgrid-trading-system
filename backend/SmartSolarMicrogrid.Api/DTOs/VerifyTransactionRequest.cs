using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class VerifyTransactionRequest
{
    [Required]
    public string? TransactionReference { get; init; }
}
