/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: UpdateOperatorSlotAvailabilityRequest.cs
 * Component: Operator Energy Slot Workflow
 *
 * Component Owners:
 * - R A K Hansika (IT23140998) - energy-slot availability data
 * - Kulunu Kasthuri Arachchi (IT23375628) - operator availability update request
 *
 * Purpose:
 * Defines the operator-controlled availability field for an existing energy slot.
 */
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class UpdateOperatorSlotAvailabilityRequest
{
    [Required]
    public bool? IsAvailable { get; init; }
}
