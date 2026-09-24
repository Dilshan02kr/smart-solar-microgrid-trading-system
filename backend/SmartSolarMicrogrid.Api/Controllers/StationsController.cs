using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.Api.Common;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Services;

namespace SmartSolarMicrogrid.Api.Controllers;

[ApiController]
[Route("api/stations")]
public sealed class StationsController(IStationManagementService stationService) : ControllerBase
{
    [Authorize]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<StationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StationResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var stations = await stationService.GetAllAsync(cancellationToken);
        return Ok(stations);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType<StationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StationResponse>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await stationService.GetByIdAsync(id, cancellationToken);
        return MapResult(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpPost]
    [ProducesResponseType<StationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StationResponse>> Create(
        CreateStationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await stationService.CreateAsync(request, cancellationToken);
        return result.Status == StationManagementStatus.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Station!.Id }, result.Station)
            : MapResult(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpPut("{id}")]
    [ProducesResponseType<StationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StationResponse>> Update(
        string id,
        UpdateStationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await stationService.UpdateAsync(id, request, cancellationToken);
        return MapResult(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpPatch("{id}/activate")]
    [ProducesResponseType<StationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StationResponse>> Activate(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await stationService.ActivateAsync(id, cancellationToken);
        return MapResult(result);
    }

    [Authorize(Roles = nameof(UserRole.BACKOFFICE))]
    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType<StationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StationResponse>> Deactivate(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await stationService.DeactivateAsync(id, cancellationToken);
        return MapResult(result);
    }

    private ActionResult<StationResponse> MapResult(StationManagementResult result) =>
        result.Status switch
        {
            StationManagementStatus.Success => Ok(result.Station),
            StationManagementStatus.InvalidId => BadRequest(new ApiErrorResponse(
                "INVALID_STATION_ID",
                "The Station ID must be a valid MongoDB ObjectId.")),
            StationManagementStatus.NotFound => NotFound(new ApiErrorResponse(
                "STATION_NOT_FOUND",
                "The requested Station was not found.")),
            StationManagementStatus.ValidationError => BadRequest(new ApiErrorResponse(
                "VALIDATION_ERROR",
                result.ErrorMessage ?? "The request payload validation failed.")),
            StationManagementStatus.DeactivationUnavailable => Conflict(new ApiErrorResponse(
                "DEACTIVATION_UNAVAILABLE",
                result.ErrorMessage ?? "Station deactivation is currently unavailable.")),
            StationManagementStatus.HasActiveReservations => Conflict(new ApiErrorResponse(
                "ACTIVE_RESERVATIONS_EXIST",
                result.ErrorMessage ?? "Station cannot be deactivated while active reservations exist.")),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiErrorResponse("SERVER_ERROR", "An unexpected error occurred."))
        };
}
