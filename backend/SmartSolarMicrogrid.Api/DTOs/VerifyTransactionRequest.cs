/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: VerifyTransactionRequest.cs
 * Component: QR Verification and Transaction Completion
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Accepts a server-issued transaction reference for operator verification.
 */
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class VerifyTransactionRequest
{
    [Required]
    public string? TransactionReference { get; init; }
}
