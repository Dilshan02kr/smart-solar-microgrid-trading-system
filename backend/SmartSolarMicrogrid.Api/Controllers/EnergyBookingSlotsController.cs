// Provides authenticated API endpoints for reading and managing energy booking slots.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.Api.Common;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Services;

namespace SmartSolarMicrogrid.Api.Controllers;

[ApiController]
[Route("api/energy-booking-slots")]
[Authorize]
public sealed class EnergyBookingSlotsController(
    IEnergyBookingSlotService slotService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EnergyBookingSlotResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EnergyBookingSlotResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        // Return every slot to an authenticated active user.
        return Ok(await slotService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{slotId}")]
    [ProducesResponseType<EnergyBookingSlotResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnergyBookingSlotResponse>> GetById(
        string slotId,
        CancellationToken cancellationToken)
    {
        // Return one slot after validating its public string identifier.
        var result = await slotService.GetByIdAsync(slotId, cancellationToken);
        return MapSingleResult(result);
    }

    [HttpGet("station/{stationId}")]
    [ProducesResponseType<IReadOnlyList<EnergyBookingSlotResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<EnergyBookingSlotResponse>>> GetByStationId(
        string stationId,
        CancellationToken cancellationToken)
    {
        // Return slots belonging to one validated, existing station.
        var result = await slotService.GetByStationIdAsync(stationId, cancellationToken);
        return result.Status == EnergyBookingSlotManagementStatus.Success
            ? Ok(result.Slots)
            : MapListError(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpPost]
    [ProducesResponseType<EnergyBookingSlotResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EnergyBookingSlotResponse>> Create(
        CreateEnergyBookingSlotRequest request,
        CancellationToken cancellationToken)
    {
        // Create a validated slot while keeping identity and availability server controlled.
        var result = await slotService.CreateAsync(request, cancellationToken);
        return result.Status == EnergyBookingSlotManagementStatus.Success
            ? CreatedAtAction(nameof(GetById), new { slotId = result.Slot!.Id }, result.Slot)
            : MapSingleResult(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpPut("{slotId}")]
    [ProducesResponseType<EnergyBookingSlotResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EnergyBookingSlotResponse>> Update(
        string slotId,
        UpdateEnergyBookingSlotRequest request,
        CancellationToken cancellationToken)
    {
        // Update only the mutable date, time, and capacity fields of an existing slot.
        var result = await slotService.UpdateAsync(slotId, request, cancellationToken);
        return MapSingleResult(result);
    }

    private ActionResult<EnergyBookingSlotResponse> MapSingleResult(
        EnergyBookingSlotManagementResult result)
    {
        // Convert service outcomes into structured HTTP responses for single-slot operations.
        return result.Status switch
        {
            EnergyBookingSlotManagementStatus.Success => Ok(result.Slot),
            EnergyBookingSlotManagementStatus.InvalidSlotId => BadRequest(Error(
                "INVALID_SLOT_ID", "The slot ID must be a valid MongoDB ObjectId.")),
            EnergyBookingSlotManagementStatus.InvalidStationId => BadRequest(Error(
                "INVALID_STATION_ID", "The station ID must be a valid MongoDB ObjectId.")),
            EnergyBookingSlotManagementStatus.SlotNotFound => NotFound(Error(
                "SLOT_NOT_FOUND", "The requested slot was not found.")),
            EnergyBookingSlotManagementStatus.StationNotFound => NotFound(Error(
                "STATION_NOT_FOUND", "The referenced station was not found.")),
            EnergyBookingSlotManagementStatus.InvalidSlotTime => BadRequest(Error(
                "INVALID_SLOT_TIME", "The slot date and time range are invalid.")),
            EnergyBookingSlotManagementStatus.InvalidSlotCapacity => BadRequest(Error(
                "INVALID_SLOT_CAPACITY", "Slot capacity must be a finite value greater than zero.")),
            EnergyBookingSlotManagementStatus.SlotCapacityExceedsStation => BadRequest(Error(
                "SLOT_CAPACITY_EXCEEDS_STATION", "Slot capacity cannot exceed station capacity.")),
            EnergyBookingSlotManagementStatus.SlotTimeConflict => Conflict(Error(
                "SLOT_TIME_CONFLICT", "The slot overlaps an existing slot for this station and date.")),
            _ => StatusCode(StatusCodes.Status500InternalServerError, Error(
                "SERVER_ERROR", "An unexpected error occurred."))
        };
    }

    private ActionResult<IReadOnlyList<EnergyBookingSlotResponse>> MapListError(
        EnergyBookingSlotManagementResult result)
    {
        // Convert station-list failures into structured HTTP responses.
        return result.Status switch
        {
            EnergyBookingSlotManagementStatus.InvalidStationId => BadRequest(Error(
                "INVALID_STATION_ID", "The station ID must be a valid MongoDB ObjectId.")),
            EnergyBookingSlotManagementStatus.StationNotFound => NotFound(Error(
                "STATION_NOT_FOUND", "The referenced station was not found.")),
            _ => StatusCode(StatusCodes.Status500InternalServerError, Error(
                "SERVER_ERROR", "An unexpected error occurred."))
        };
    }

    private static ApiErrorResponse Error(string code, string message)
    {
        // Construct the established structured API error contract.
        return new ApiErrorResponse(code, message);
    }
}
