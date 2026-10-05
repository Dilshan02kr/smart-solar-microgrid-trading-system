/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: ProsumerLoginRequest.cs
 * Component: Prosumer Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines NIC and password credentials for Prosumer login.
 */
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class ProsumerLoginRequest
{
    [Required]
    public string? Nic { get; init; }

    [Required]
    public string? Password { get; init; }
}
