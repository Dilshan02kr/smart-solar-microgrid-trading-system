/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: CreateEnergyBookingSlotRequest.cs
 * Component: Energy Slot Management
 * Component Owner: R A K Hansika (IT23140998)
 *
 * Purpose:
 * Defines client-controlled fields for creating an energy booking slot.
 */
using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.Api.DTOs;

public sealed class CreateEnergyBookingSlotRequest
{
    [Required(ErrorMessage = "Station ID is required.")]
    public string? StationId { get; init; }

    [Required(ErrorMessage = "Date is required.")]
    public DateTime? Date { get; init; }

    [Required(ErrorMessage = "Start time is required.")]
    public TimeSpan? StartTime { get; init; }

    [Required(ErrorMessage = "End time is required.")]
    public TimeSpan? EndTime { get; init; }

    [Required(ErrorMessage = "Capacity is required.")]
    public double? CapacityKw { get; init; }
}
