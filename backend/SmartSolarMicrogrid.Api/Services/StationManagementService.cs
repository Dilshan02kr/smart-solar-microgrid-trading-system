using MongoDB.Bson;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class StationManagementService(
    ISolarStationRepository stationRepository) : IStationManagementService
{
    public async Task<IReadOnlyList<StationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var stations = await stationRepository.GetAllAsync(cancellationToken);
        return stations.Select(MapStation).ToList();
    }

    public async Task<StationManagementResult> GetByIdAsync(
        string stationId,
        CancellationToken cancellationToken = default)
    {
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
        if (!ObjectId.TryParse(stationId, out _))
        {
            return new StationManagementResult(StationManagementStatus.InvalidId);
        }

        var updatedStation = await stationRepository.UpdateStatusAsync(
            stationId,
            StationStatus.ACTIVE,
            cancellationToken);

        return updatedStation is not null
            ? new StationManagementResult(StationManagementStatus.Success, MapStation(updatedStation))
            : new StationManagementResult(StationManagementStatus.NotFound);
    }

    public async Task<StationManagementResult> DeactivateAsync(
        string stationId,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(stationId, out _))
        {
            return new StationManagementResult(StationManagementStatus.InvalidId);
        }

        var existingStation = await stationRepository.GetByIdAsync(stationId, cancellationToken);
        if (existingStation is null)
        {
            return new StationManagementResult(StationManagementStatus.NotFound);
        }

        // Station deactivation rule: A station cannot be deactivated while active reservations exist.
        // Temporarily returns DeactivationUnavailable until Member 3 reservation integration is completed.
        return new StationManagementResult(
            StationManagementStatus.DeactivationUnavailable,
            ErrorMessage: "Station deactivation is currently unavailable until Member 3 active-reservation validation is integrated.");
    }

    private static string? ValidateRequest(
        string? name,
        string? locationName,
        double? latitude,
        double? longitude,
        double? totalCapacityKw,
        string? operationalSchedule)
    {
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
