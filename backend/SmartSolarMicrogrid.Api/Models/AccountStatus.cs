/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: AccountStatus.cs
 * Component: Authentication and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Defines the controlled lifecycle states of a user account.
 */
namespace SmartSolarMicrogrid.Api.Models;

public enum AccountStatus
{
    PENDING,
    ACTIVE,
    DEACTIVATED
}
