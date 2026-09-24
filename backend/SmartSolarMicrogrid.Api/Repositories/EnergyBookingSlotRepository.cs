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
        _slots = database.GetCollection<EnergyBookingSlot>(CollectionName);
    }

    public async Task CreateAsync(
        EnergyBookingSlot slot,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        slot.CreatedAt = now;
        slot.UpdatedAt = now;

        await _slots.InsertOneAsync(slot, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<EnergyBookingSlot>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _slots
            .Find(Builders<EnergyBookingSlot>.Filter.Empty)
            .SortByDescending(slot => slot.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<EnergyBookingSlot?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
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

    public async Task<EnergyBookingSlot?> UpdateAsync(
        string id,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        double capacityKw,
        CancellationToken cancellationToken = default)
    {
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
