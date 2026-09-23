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

        // 2a. Get a single reservation by transaction reference
        public async Task<EnergyReservation?> GetByTransactionReferenceAsync(string transactionReference) =>
            await _reservationsCollection.Find(r => r.TransactionReference == transactionReference).FirstOrDefaultAsync();

        // 2b. Atomically complete an approved reservation to prevent race conditions
        public async Task<EnergyReservation?> TryCompleteApprovedReservationAsync(string reservationId)
        {
            var filter = Builders<EnergyReservation>.Filter.And(
                Builders<EnergyReservation>.Filter.Eq(r => r.Id, reservationId),
                Builders<EnergyReservation>.Filter.Eq(r => r.Status, "Approved")
            );

            var update = Builders<EnergyReservation>.Update.Set(r => r.Status, "Completed");

            var options = new FindOneAndUpdateOptions<EnergyReservation>
            {
                ReturnDocument = ReturnDocument.After
            };

            return await _reservationsCollection.FindOneAndUpdateAsync(filter, update, options);
        }

        // 3. Count reservations by exact status (server-side, no in-memory load)
        public async Task<long> CountByStatusAsync(string status) =>
            await _reservationsCollection.CountDocumentsAsync(
                Builders<EnergyReservation>.Filter.Eq(r => r.Status, status));

        // 3a. Count approved reservations whose ScheduledTime is in the future
        public async Task<long> CountApprovedFutureAsync()
        {
            var filter = Builders<EnergyReservation>.Filter.And(
                Builders<EnergyReservation>.Filter.Eq(r => r.Status, "Approved"),
                Builders<EnergyReservation>.Filter.Gt(r => r.ScheduledTime, DateTime.UtcNow)
            );
            return await _reservationsCollection.CountDocumentsAsync(filter);
        }

        // 4. Save a new reservation
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