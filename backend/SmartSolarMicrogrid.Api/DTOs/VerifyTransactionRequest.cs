// Accepts a server-issued transaction reference for operator verification.
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class VerifyTransactionRequest
{
    [Required]
    public string? TransactionReference { get; init; }
}
