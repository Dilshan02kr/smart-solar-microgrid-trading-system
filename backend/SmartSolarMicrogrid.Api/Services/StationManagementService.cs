/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: StationManagementService.cs
 * Component: Microgrid Node and Station Management
 * Component Owner: R A K Hansika (IT23140998)
 *
 * Purpose:
 * Implements validated station management and active-reservation deactivation protection.
 */
using MongoDB.Bson;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class StationManagementService(
    ISolarStationRepository stationRepository,
    IEnergyReservationRepository reservationRepository) : IStationManagementService
{
    public async Task<IReadOnlyList<StationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // Returns all persisted stations as public response DTOs.
        var stations = await stationRepository.GetAllAsync(cancellationToken);
        return stations.Select(MapStation).ToList();
    }

    public async Task<StationManagementResult> GetByIdAsync(
        string stationId,
        CancellationToken cancellationToken = default)
    {
        // Validates and retrieves one station by its public ObjectId string.
        if (!ObjectId.TryParse(stationId, out _))
        {
            return new StationManagementResult(StationManagementStatus.InvalidId);
        }

        var station = await stationRepository.GetByIdAsync(stationId, cancellationToken);
        return station is not null
            ? new StationManagementResult(StationManagementStatus.Success, MapStation(station))
            : new StationManagementResult(StationManagementStatus.NotFound);
    }

    public async Task<StationManagementResult> CreateAsync(
        CreateStationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validates client fields and creates a server-owned active station.
        var validationError = ValidateRequest(
            request.Name,
            request.LocationName,
            request.Latitude,
            request.Longitude,
            request.TotalCapacityKw,
            request.OperationalSchedule);

        if (validationError is not null)
        {
            return new StationManagementResult(
                StationManagementStatus.ValidationError,
                ErrorMessage: validationError);
        }

        var station = new SolarStationInfo
        {
            Name = request.Name!.Trim(),
            LocationName = request.LocationName!.Trim(),
            Latitude = request.Latitude!.Value,
            Longitude = request.Longitude!.Value,
            TotalCapacityKw = request.TotalCapacityKw!.Value,
            OperationalSchedule = request.OperationalSchedule!.Trim(),
            Status = StationStatus.ACTIVE
        };

        await stationRepository.CreateAsync(station, cancellationToken);
        return new StationManagementResult(StationManagementStatus.Success, MapStation(station));
    }

    public async Task<StationManagementResult> UpdateAsync(
        string stationId,
        UpdateStationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validates and updates the mutable details of an existing station.
        if (!ObjectId.TryParse(stationId, out _))
        {
            return new StationManagementResult(StationManagementStatus.InvalidId);
        }

        var validationError = ValidateRequest(
            request.Name,
            request.LocationName,
            request.Latitude,
            request.Longitude,
            request.TotalCapacityKw,
            request.OperationalSchedule);

        if (validationError is not null)
        {
            return new StationManagementResult(
                StationManagementStatus.ValidationError,
                ErrorMessage: validationError);
        }

        var updatedStation = await stationRepository.UpdateAsync(
            stationId,
            request.Name!.Trim(),
            request.LocationName!.Trim(),
            request.Latitude!.Value,
            request.Longitude!.Value,
            request.TotalCapacityKw!.Value,
            request.OperationalSchedule!.Trim(),
            cancellationToken);

        return updatedStation is not null
            ? new StationManagementResult(StationManagementStatus.Success, MapStation(updatedStation))
            : new StationManagementResult(StationManagementStatus.NotFound);
    }

    public async Task<StationManagementResult> ActivateAsync(
        string stationId,
        CancellationToken cancellationToken = default)
    {
        // Activates only an existing inactive station through an expected-state transition.
        if (!ObjectId.TryParse(stationId, out _))
        {
            return new StationManagementResult(StationManagementStatus.InvalidId);
        }

        var existingStation = await stationRepository.GetByIdAsync(stationId, cancellationToken);
        if (existingStation is null)
        {
            return new StationManagementResult(StationManagementStatus.NotFound);
        }

        if (existingStation.Status != StationStatus.INACTIVE)
        {
            return new StationManagementResult(
                StationManagementStatus.InvalidState,
                ErrorMessage: "Only an inactive station can be activated.");
        }

        var updatedStation = await stationRepository.UpdateStatusAsync(
            stationId,
            StationStatus.INACTIVE,
            StationStatus.ACTIVE,
            cancellationToken);

        return updatedStation is not null
            ? new StationManagementResult(StationManagementStatus.Success, MapStation(updatedStation))
            : new StationManagementResult(
                StationManagementStatus.InvalidState,
                ErrorMessage: "The station state changed before activation completed.");
    }

    public async Task<StationManagementResult> DeactivateAsync(
        string stationId,
        CancellationToken cancellationToken = default)
    {
        // Deactivate an active station only when it has no pending or approved reservations.
        if (!ObjectId.TryParse(stationId, out _))
        {
            return new StationManagementResult(StationManagementStatus.InvalidId);
        }

        var existingStation = await stationRepository.GetByIdAsync(stationId, cancellationToken);
        if (existingStation is null)
        {
            return new StationManagementResult(StationManagementStatus.NotFound);
        }

        if (existingStation.Status != StationStatus.ACTIVE)
        {
            return new StationManagementResult(
                StationManagementStatus.InvalidState,
                ErrorMessage: "Only an active station can be deactivated.");
        }

        if (await reservationRepository.HasActiveReservationsForStationAsync(
                stationId,
                cancellationToken))
        {
            return new StationManagementResult(
                StationManagementStatus.HasActiveReservations,
                ErrorMessage: "Station cannot be deactivated while pending or approved reservations exist.");
        }

        var updatedStation = await stationRepository.UpdateStatusAsync(
            stationId,
            StationStatus.ACTIVE,
            StationStatus.INACTIVE,
            cancellationToken);

        return updatedStation is not null
            ? new StationManagementResult(StationManagementStatus.Success, MapStation(updatedStation))
            : new StationManagementResult(
                StationManagementStatus.InvalidState,
                ErrorMessage: "The station state changed before deactivation completed.");
    }

    private static string? ValidateRequest(
        string? name,
        string? locationName,
        double? latitude,
        double? longitude,
        double? totalCapacityKw,
        string? operationalSchedule)
    {
        // Validates required station text, geographic bounds, and positive capacity.
        if (string.IsNullOrWhiteSpace(name))
            return "Station name cannot be empty or whitespace.";

        if (string.IsNullOrWhiteSpace(locationName))
            return "Location name cannot be empty or whitespace.";

        if (string.IsNullOrWhiteSpace(operationalSchedule))
            return "Operational schedule cannot be empty or whitespace.";

        if (latitude is null or < -90.0 or > 90.0)
            return "Latitude must be between -90 and +90 degrees.";

        if (longitude is null or < -180.0 or > 180.0)
            return "Longitude must be between -180 and +180 degrees.";

        if (totalCapacityKw is null or <= 0.0)
            return "Total capacity must be greater than 0 kW.";

        return null;
    }

    // Maps a station document to the public response contract.
    private static StationResponse MapStation(SolarStationInfo station) =>
        new(
            station.Id.ToString(),
            station.Name,
            station.LocationName,
            station.Latitude,
            station.Longitude,
            station.TotalCapacityKw,
            station.OperationalSchedule,
            station.Status.ToString(),
            station.CreatedAt,
            station.UpdatedAt);
}
