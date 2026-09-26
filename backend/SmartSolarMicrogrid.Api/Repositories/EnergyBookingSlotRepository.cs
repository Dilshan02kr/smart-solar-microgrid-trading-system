// Provides MongoDB persistence and atomic availability operations for energy booking slots.
using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Repositories;

public sealed class EnergyBookingSlotRepository : IEnergyBookingSlotRepository
{
    public const string CollectionName = "EnergyBookingSlots";

    private readonly IMongoCollection<EnergyBookingSlot> _slots;

    public EnergyBookingSlotRepository(IMongoDatabase database)
    {
        // Resolves the established energy-slot collection from the shared database.
        _slots = database.GetCollection<EnergyBookingSlot>(CollectionName);
    }

    public async Task CreateAsync(
        EnergyBookingSlot slot,
        CancellationToken cancellationToken = default)
    {
        // Applies server UTC timestamps and inserts a new slot document.
        var now = DateTime.UtcNow;
        slot.CreatedAt = now;
        slot.UpdatedAt = now;

        await _slots.InsertOneAsync(slot, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<EnergyBookingSlot>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // Returns all slots ordered by newest creation time.
        return await _slots
            .Find(Builders<EnergyBookingSlot>.Filter.Empty)
            .SortByDescending(slot => slot.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<EnergyBookingSlot?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // Validates an ObjectId string and retrieves the matching slot.
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        return await _slots
            .Find(slot => slot.Id == objectId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EnergyBookingSlot>> GetByStationIdAsync(
        string stationId,
        CancellationToken cancellationToken = default)
    {
        // Returns chronologically ordered slots belonging to one validated station.
        if (!ObjectId.TryParse(stationId, out var stationObjectId))
        {
            return Array.Empty<EnergyBookingSlot>();
        }

        return await _slots
            .Find(slot => slot.StationId == stationObjectId)
            .SortBy(slot => slot.Date)
            .ThenBy(slot => slot.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlappingSlotAsync(
        ObjectId stationId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        ObjectId? excludedSlotId = null,
        CancellationToken cancellationToken = default)
    {
        // Check whether another slot intersects the requested station, date, and time range.
        var builder = Builders<EnergyBookingSlot>.Filter;
        var filter = builder.Eq(slot => slot.StationId, stationId) &
                     builder.Eq(slot => slot.Date, date) &
                     builder.Lt(slot => slot.StartTime, endTime) &
                     builder.Gt(slot => slot.EndTime, startTime);

        if (excludedSlotId.HasValue)
        {
            filter &= builder.Ne(slot => slot.Id, excludedSlotId.Value);
        }

        return await _slots.Find(filter).AnyAsync(cancellationToken);
    }

    public async Task<bool> TryClaimAvailableAsync(
        string slotId,
        CancellationToken cancellationToken = default)
    {
        // Atomically change an available slot to unavailable for one reservation creator.
        if (!ObjectId.TryParse(slotId, out var objectId))
        {
            return false;
        }

        var filter = Builders<EnergyBookingSlot>.Filter.Eq(slot => slot.Id, objectId) &
                     Builders<EnergyBookingSlot>.Filter.Eq(slot => slot.IsAvailable, true);
        var update = Builders<EnergyBookingSlot>.Update
            .Set(slot => slot.IsAvailable, false)
            .Set(slot => slot.UpdatedAt, DateTime.UtcNow);
        var result = await _slots.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount == 1;
    }

    public async Task<bool> ReleaseAsync(
        string slotId,
        CancellationToken cancellationToken = default)
    {
        // Mark an existing slot available after a reservation releases it.
        if (!ObjectId.TryParse(slotId, out var objectId))
        {
            return false;
        }

        var filter = Builders<EnergyBookingSlot>.Filter.Eq(slot => slot.Id, objectId);
        var update = Builders<EnergyBookingSlot>.Update
            .Set(slot => slot.IsAvailable, true)
            .Set(slot => slot.UpdatedAt, DateTime.UtcNow);
        var result = await _slots.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.MatchedCount == 1;
    }

    public async Task<EnergyBookingSlot?> UpdateAsync(
        string id,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        double capacityKw,
        CancellationToken cancellationToken = default)
    {
        // Updates mutable slot timing and capacity fields with a server UTC timestamp.
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var filter = Builders<EnergyBookingSlot>.Filter.Eq(slot => slot.Id, objectId);
        var update = Builders<EnergyBookingSlot>.Update
            .Set(slot => slot.Date, date)
            .Set(slot => slot.StartTime, startTime)
            .Set(slot => slot.EndTime, endTime)
            .Set(slot => slot.CapacityKw, capacityKw)
            .Set(slot => slot.UpdatedAt, DateTime.UtcNow);

        return await _slots.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<EnergyBookingSlot>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);
    }
}
