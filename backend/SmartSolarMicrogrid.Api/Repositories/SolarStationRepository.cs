// Provides MongoDB persistence and atomic status changes for solar stations.
using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Repositories;

public sealed class SolarStationRepository : ISolarStationRepository
{
    public const string CollectionName = "SolarStationInfo";

    private readonly IMongoCollection<SolarStationInfo> _stations;

    public SolarStationRepository(IMongoDatabase database)
    {
        // Resolves the established station collection from the shared database.
        _stations = database.GetCollection<SolarStationInfo>(CollectionName);
    }

    public async Task CreateAsync(
        SolarStationInfo station,
        CancellationToken cancellationToken = default)
    {
        // Applies server UTC timestamps and inserts a new station document.
        var now = DateTime.UtcNow;
        station.CreatedAt = now;
        station.UpdatedAt = now;

        await _stations.InsertOneAsync(station, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<SolarStationInfo>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // Returns all stations ordered by newest creation time.
        return await _stations
            .Find(Builders<SolarStationInfo>.Filter.Empty)
            .SortByDescending(station => station.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SolarStationInfo?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // Validates an ObjectId string and retrieves the matching station.
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        return await _stations
            .Find(station => station.Id == objectId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SolarStationInfo?> UpdateAsync(
        string id,
        string name,
        string locationName,
        double latitude,
        double longitude,
        double totalCapacityKw,
        string operationalSchedule,
        CancellationToken cancellationToken = default)
    {
        // Updates mutable station details while preserving identity and lifecycle fields.
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var filter = Builders<SolarStationInfo>.Filter.Eq(station => station.Id, objectId);
        var update = Builders<SolarStationInfo>.Update
            .Set(station => station.Name, name)
            .Set(station => station.LocationName, locationName)
            .Set(station => station.Latitude, latitude)
            .Set(station => station.Longitude, longitude)
            .Set(station => station.TotalCapacityKw, totalCapacityKw)
            .Set(station => station.OperationalSchedule, operationalSchedule)
            .Set(station => station.UpdatedAt, DateTime.UtcNow);

        return await _stations.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<SolarStationInfo>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);
    }

    public async Task<SolarStationInfo?> UpdateStatusAsync(
        string id,
        StationStatus expectedStatus,
        StationStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        // Applies an atomic expected-state station transition with a server timestamp.
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var filter = Builders<SolarStationInfo>.Filter.Eq(station => station.Id, objectId) &
                     Builders<SolarStationInfo>.Filter.Eq(
                         station => station.Status,
                         expectedStatus);
        var update = Builders<SolarStationInfo>.Update
            .Set(station => station.Status, newStatus)
            .Set(station => station.UpdatedAt, DateTime.UtcNow);

        return await _stations.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<SolarStationInfo>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);
    }
}
