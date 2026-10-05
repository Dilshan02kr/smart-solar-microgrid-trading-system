/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: UserRole.cs
 * Component: Web User and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines the authoritative application roles used by authentication and authorization.
 */
namespace SmartSolarMicrogrid.Api.Models;

public enum UserRole
{
    BACKOFFICE,
    GRID_OPERATOR,
    PROSUMER
}
