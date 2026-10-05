/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: ApiErrorResponse.cs
 * Component: API Integration
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Defines the structured error payload returned consistently by API endpoints.
 */
namespace SmartSolarMicrogrid.Api.Common;

public sealed record ApiErrorResponse(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Errors = null);
