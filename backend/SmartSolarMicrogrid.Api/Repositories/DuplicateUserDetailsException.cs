/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: DuplicateUserDetailsException.cs
 * Component: Web User and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Identifies MongoDB unique-index conflicts encountered while persisting user accounts.
 */
namespace SmartSolarMicrogrid.Api.Repositories;

public enum DuplicateUserField
{
    Email,
    Nic,
    Unknown
}

public sealed class DuplicateUserDetailsException(
    DuplicateUserField field,
    Exception innerException) : Exception("A unique UserDetails field already exists.", innerException)
{
    public DuplicateUserField Field { get; } = field;
}
