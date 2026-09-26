// Defines MongoDB persistence operations required by station-management workflows.
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Repositories;

public interface ISolarStationRepository
{
    // Inserts a new server-controlled station document.
    Task CreateAsync(
        SolarStationInfo station,
        CancellationToken cancellationToken = default);

    // Returns all persisted stations.
    Task<IReadOnlyList<SolarStationInfo>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // Retrieves one station by a validated public identifier.
    Task<SolarStationInfo?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    // Updates the mutable descriptive and capacity fields of a station.
    Task<SolarStationInfo?> UpdateAsync(
        string id,
        string name,
        string locationName,
        double latitude,
        double longitude,
        double totalCapacityKw,
        string operationalSchedule,
        CancellationToken cancellationToken = default);

    // Atomically performs an expected-state station status transition.
    Task<SolarStationInfo?> UpdateStatusAsync(
        string id,
        StationStatus expectedStatus,
        StationStatus newStatus,
        CancellationToken cancellationToken = default);
}
