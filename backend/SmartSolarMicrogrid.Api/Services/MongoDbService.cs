using MongoDB.Driver;
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<EnergyReservation> _reservationsCollection;

        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDb:ConnectionString"];
            var databaseName = configuration["MongoDb:DatabaseName"];
            var collectionName = configuration["MongoDb:ReservationsCollectionName"] ?? "EnergyReservations";

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _reservationsCollection = database.GetCollection<EnergyReservation>(collectionName);
        }

        // 1. Get all reservations for a specific prosumer
        public async Task<List<EnergyReservation>> GetByProsumerAsync(string prosumerId) =>
            await _reservationsCollection.Find(r => r.ProsumerId == prosumerId).ToListAsync();

        // 2. Get a single reservation by ID
        public async Task<EnergyReservation?> GetByIdAsync(string id) =>
            await _reservationsCollection.Find(r => r.Id == id).FirstOrDefaultAsync();

        // 3. Save a new reservation
        public async Task CreateAsync(EnergyReservation reservation) =>
            await _reservationsCollection.InsertOneAsync(reservation);

        // 4. Update an existing reservation
        public async Task UpdateAsync(string id, EnergyReservation updatedReservation) =>
            await _reservationsCollection.ReplaceOneAsync(r => r.Id == id, updatedReservation);

        // 5. Search / Filter
        public async Task<List<EnergyReservation>> GetFilteredAsync(string prosumerId, string? status, string? searchTerm)
        {
            var builder = Builders<EnergyReservation>.Filter;
            var filter = builder.Eq(r => r.ProsumerId, prosumerId);

            if (!string.IsNullOrEmpty(status))
            {
                filter &= builder.Eq(r => r.Status, status);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filter &= (builder.Regex(r => r.StationId, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")) |
                           builder.Regex(r => r.TransactionReference, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")));
            }

            return await _reservationsCollection.Find(filter).SortByDescending(r => r.ScheduledTime).ToListAsync();
        }
    }
}