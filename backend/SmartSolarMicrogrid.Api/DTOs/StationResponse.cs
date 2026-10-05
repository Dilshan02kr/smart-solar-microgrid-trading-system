/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: StationResponse.cs
 * Component: Microgrid Node and Station Management
 * Component Owner: R A K Hansika (IT23140998)
 *
 * Purpose:
 * Exposes safe solar-station details through the public API contract.
 */
namespace SmartSolarMicrogrid.Api.DTOs;

public sealed record StationResponse(
    string Id,
    string Name,
    string LocationName,
    double Latitude,
    double Longitude,
    double TotalCapacityKw,
    string OperationalSchedule,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
