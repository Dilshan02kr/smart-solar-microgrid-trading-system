/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: WebLoginRequest.cs
 * Component: Authentication and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines email and password credentials for Backoffice and Grid Operator login.
 */
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class WebLoginRequest
{
    [Required]
    public string? Email { get; init; }

    [Required]
    public string? Password { get; init; }
}
