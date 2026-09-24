using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Repositories;

public interface ISolarStationRepository
{
    Task CreateAsync(
        SolarStationInfo station,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SolarStationInfo>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SolarStationInfo?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<SolarStationInfo?> UpdateAsync(
        string id,
        string name,
        string locationName,
        double latitude,
        double longitude,
        double totalCapacityKw,
        string operationalSchedule,
        CancellationToken cancellationToken = default);

    Task<SolarStationInfo?> UpdateStatusAsync(
        string id,
        StationStatus newStatus,
        CancellationToken cancellationToken = default);
}
