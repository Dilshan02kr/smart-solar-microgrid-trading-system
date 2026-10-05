/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: OperatorErrorCodes.cs
 * Component: Grid Operator Workflow
 * Component Owner: Kulunu Kasthuri Arachchi (IT23375628)
 *
 * Purpose:
 * Defines stable error codes returned by assigned-station operator workflows.
 */
namespace SmartSolarMicrogrid.Api.Common;

public static class OperatorErrorCodes
{
    public const string StationNotAssigned = "OPERATOR_STATION_NOT_ASSIGNED";
    public const string SlotHasActiveReservation = "SLOT_HAS_ACTIVE_RESERVATION";
}
