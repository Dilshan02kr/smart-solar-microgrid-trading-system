using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<EnergyReservation> _reservationsCollection;

        // Use IMongoDatabase directly from Dependency Injection (registered in Program.cs)
        public MongoDbService(IMongoDatabase database, IConfiguration configuration)
        {
            var collectionName = configuration["MongoDb:ReservationsCollectionName"] ?? "EnergyReservations";
            _reservationsCollection = database.GetCollection<EnergyReservation>(collectionName);
        }

        // 1. Get all reservations for a specific prosumer
        public async Task<List<EnergyReservation>> GetByProsumerAsync(string prosumerId) =>
            await _reservationsCollection.Find(r => r.ProsumerId == prosumerId).SortByDescending(r => r.ScheduledTime).ToListAsync();

        // 2. Get a single reservation by ID
        public async Task<EnergyReservation?> GetByIdAsync(string id) =>
            await _reservationsCollection.Find(r => r.Id == id).FirstOrDefaultAsync();

        // 3. Save a new reservation
        public async Task CreateAsync(EnergyReservation reservation) =>
            await _reservationsCollection.InsertOneAsync(reservation);

        // 4. Update an existing reservation
        public async Task UpdateAsync(string id, EnergyReservation updatedReservation) =>
            await _reservationsCollection.ReplaceOneAsync(r => r.Id == id, updatedReservation);

        // 5. Member 2 Requirement: Check if station has active (Pending/Approved) reservations
        public async Task<bool> HasActiveReservationsAsync(string stationId)
        {
            var activeStatuses = new[] { "Pending", "Approved" };
            var filter = Builders<EnergyReservation>.Filter.Eq(r => r.StationId, stationId) &
                         Builders<EnergyReservation>.Filter.In(r => r.Status, activeStatuses);

            return await _reservationsCollection.Find(filter).AnyAsync();
        }

        // 6. Search / Filter with exact Status & Keyword matching
        public async Task<List<EnergyReservation>> GetFilteredAsync(string prosumerId, string? status, string? searchTerm)
        {
            var builder = Builders<EnergyReservation>.Filter;
            var filter = builder.Eq(r => r.ProsumerId, prosumerId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                filter &= builder.Eq(r => r.Status, status);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var regex = new BsonRegularExpression(searchTerm, "i");
                var searchFilter = builder.Regex(r => r.StationId, regex) |
                                   builder.Regex(r => r.SlotId, regex) |
                                   builder.Regex(r => r.TransactionReference, regex);
                filter &= searchFilter;
            }

            return await _reservationsCollection.Find(filter).SortByDescending(r => r.ScheduledTime).ToListAsync();
        }
    }
}