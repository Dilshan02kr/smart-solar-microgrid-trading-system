// Defines the authoritative application roles used by authentication and authorization.
namespace SmartSolarMicrogrid.Api.Models;

public enum UserRole
{
    BACKOFFICE,
    GRID_OPERATOR,
    PROSUMER
}
