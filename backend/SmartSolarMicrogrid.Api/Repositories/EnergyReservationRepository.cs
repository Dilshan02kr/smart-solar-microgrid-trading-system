// Provides focused, concurrency-aware MongoDB access for energy reservations.
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolarMicrogrid.Api.Models;

namespace SmartSolarMicrogrid.Api.Repositories;

public sealed class EnergyReservationRepository : IEnergyReservationRepository
{
    public const string CollectionName = "EnergyReservations";

    private readonly IMongoCollection<EnergyReservation> _reservations;

    public EnergyReservationRepository(IMongoDatabase database, IConfiguration configuration)
    {
        // Resolve the configured reservation collection while retaining the established default.
        var collectionName = configuration["MongoDb:ReservationsCollectionName"] ?? CollectionName;
        _reservations = database.GetCollection<EnergyReservation>(collectionName);
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        // Enforce uniqueness only when a transaction reference is present.
        var transactionReferenceIndex = new CreateIndexModel<EnergyReservation>(
            Builders<EnergyReservation>.IndexKeys.Ascending(reservation => reservation.TransactionReference),
            new CreateIndexOptions<EnergyReservation>
            {
                Name = "ux_energyreservations_transaction_reference_when_present",
                Unique = true,
                PartialFilterExpression = new BsonDocument(
                    nameof(EnergyReservation.TransactionReference),
                    new BsonDocument("$type", "string"))
            });

        await _reservations.Indexes.CreateOneAsync(
            transactionReferenceIndex,
            cancellationToken: cancellationToken);
    }

    public async Task CreateAsync(
        EnergyReservation reservation,
        CancellationToken cancellationToken = default)
    {
        // Set server timestamps and insert the reservation.
        var now = DateTime.UtcNow;
        reservation.CreatedAt = now;
        reservation.UpdatedAt = now;
        await _reservations.InsertOneAsync(reservation, cancellationToken: cancellationToken);
    }

    public async Task<EnergyReservation?> GetByIdAsync(
        string reservationId,
        CancellationToken cancellationToken = default)
    {
        // Retrieve one reservation after rejecting malformed ObjectId strings.
        if (!ObjectId.TryParse(reservationId, out _))
        {
            return null;
        }

        return await _reservations
            .Find(reservation => reservation.Id == reservationId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EnergyReservation?> GetByTransactionReferenceAsync(
        string transactionReference,
        CancellationToken cancellationToken = default)
    {
        // Retrieve the reservation associated with an exact transaction reference.
        return await _reservations
            .Find(reservation => reservation.TransactionReference == transactionReference)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EnergyReservation>> GetByProsumerAsync(
        string prosumerId,
        CancellationToken cancellationToken = default)
    {
        // Return one Prosumer's reservations with the most relevant bookings first.
        return await _reservations
            .Find(reservation => reservation.ProsumerId == prosumerId)
            .SortByDescending(reservation => reservation.ScheduledTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EnergyReservation>> GetOperationalByStationAsync(
        string stationId,
        CancellationToken cancellationToken = default)
    {
        // Never accept a client-selected station here; callers supply the resolved operator assignment.
        if (!ObjectId.TryParse(stationId, out _))
        {
            return [];
        }

        var builder = Builders<EnergyReservation>.Filter;
        var filter = builder.Eq(reservation => reservation.StationId, stationId) &
                     StatusIn(ReservationStatus.PENDING, ReservationStatus.APPROVED);
        return await _reservations
            .Find(filter)
            .SortBy(reservation => reservation.ScheduledTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EnergyReservation>> SearchAsync(
        string? prosumerId,
        ReservationStatus? status,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        // Build a bounded administrative search without accepting raw regular expressions.
        var builder = Builders<EnergyReservation>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(prosumerId))
        {
            filter &= builder.Eq(reservation => reservation.ProsumerId, prosumerId);
        }

        if (status.HasValue)
        {
            filter &= StatusIs(status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.Trim();
            var escapedReference = new BsonRegularExpression(Regex.Escape(normalizedSearch), "i");
            var searchFilter = builder.Regex(
                reservation => reservation.TransactionReference,
                escapedReference);

            if (ObjectId.TryParse(normalizedSearch, out _))
            {
                searchFilter |= builder.Eq(reservation => reservation.Id, normalizedSearch) |
                                builder.Eq(reservation => reservation.ProsumerId, normalizedSearch) |
                                builder.Eq(reservation => reservation.StationId, normalizedSearch) |
                                builder.Eq(reservation => reservation.SlotId, normalizedSearch);
            }

            filter &= searchFilter;
        }

        return await _reservations
            .Find(filter)
            .SortByDescending(reservation => reservation.ScheduledTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<EnergyReservation?> TryApprovePendingAsync(
        string reservationId,
        string transactionReference,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        // Atomically approve only a pending reservation that is still in the future.
        var builder = Builders<EnergyReservation>.Filter;
        var filter = builder.Eq(reservation => reservation.Id, reservationId) &
                     StatusIs(ReservationStatus.PENDING) &
                     builder.Gt(reservation => reservation.ScheduledTime, nowUtc);
        var update = Builders<EnergyReservation>.Update
            .Set(reservation => reservation.Status, ReservationStatus.APPROVED)
            .Set(reservation => reservation.TransactionReference, transactionReference)
            .Set(reservation => reservation.UpdatedAt, nowUtc);

        return await FindOneAndUpdateAsync(filter, update, cancellationToken);
    }

    public async Task<EnergyReservation?> TryUpdateBookingAsync(
        string reservationId,
        string prosumerId,
        string expectedSlotId,
        string stationId,
        string slotId,
        DateTime scheduledTime,
        DateTime cutoffUtc,
        DateTime updatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        // Atomically replace booking details for an owned nonterminal reservation before cutoff.
        var builder = Builders<EnergyReservation>.Filter;
        var filter = builder.Eq(reservation => reservation.Id, reservationId) &
                     builder.Eq(reservation => reservation.ProsumerId, prosumerId) &
                     builder.Eq(reservation => reservation.SlotId, expectedSlotId) &
                     StatusIn(ReservationStatus.PENDING, ReservationStatus.APPROVED) &
                     builder.Gte(reservation => reservation.ScheduledTime, cutoffUtc);
        var update = Builders<EnergyReservation>.Update
            .Set(reservation => reservation.StationId, stationId)
            .Set(reservation => reservation.SlotId, slotId)
            .Set(reservation => reservation.ScheduledTime, scheduledTime)
            .Set(reservation => reservation.Status, ReservationStatus.PENDING)
            .Set(reservation => reservation.TransactionReference, null)
            .Set(reservation => reservation.UpdatedAt, updatedAtUtc)
            .Set(reservation => reservation.CompletedAt, null);

        return await FindOneAndUpdateAsync(filter, update, cancellationToken);
    }

    public async Task<EnergyReservation?> TryCancelAsync(
        string reservationId,
        string prosumerId,
        DateTime cutoffUtc,
        DateTime updatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        // Atomically cancel an owned pending or approved reservation before cutoff.
        var builder = Builders<EnergyReservation>.Filter;
        var filter = builder.Eq(reservation => reservation.Id, reservationId) &
                     builder.Eq(reservation => reservation.ProsumerId, prosumerId) &
                     StatusIn(ReservationStatus.PENDING, ReservationStatus.APPROVED) &
                     builder.Gte(reservation => reservation.ScheduledTime, cutoffUtc);
        var update = Builders<EnergyReservation>.Update
            .Set(reservation => reservation.Status, ReservationStatus.CANCELLED)
            .Set(reservation => reservation.TransactionReference, null)
            .Set(reservation => reservation.UpdatedAt, updatedAtUtc);

        return await FindOneAndUpdateAsync(filter, update, cancellationToken);
    }

    public async Task<EnergyReservation?> TryCompleteApprovedAsync(
        string reservationId,
        string expectedStationId,
        DateTime completedAtUtc,
        CancellationToken cancellationToken = default)
    {
        // Atomically complete only an approved reservation at the operator's assigned station.
        var builder = Builders<EnergyReservation>.Filter;
        var filter = builder.Eq(reservation => reservation.Id, reservationId) &
                     builder.Eq(reservation => reservation.StationId, expectedStationId) &
                     StatusIs(ReservationStatus.APPROVED);
        var update = Builders<EnergyReservation>.Update
            .Set(reservation => reservation.Status, ReservationStatus.COMPLETED)
            .Set(reservation => reservation.UpdatedAt, completedAtUtc)
            .Set(reservation => reservation.CompletedAt, completedAtUtc);

        return await FindOneAndUpdateAsync(filter, update, cancellationToken);
    }

    public async Task<long> CountByStatusAsync(
        ReservationStatus status,
        CancellationToken cancellationToken = default)
    {
        // Count both authoritative and legacy-cased documents for the requested status.
        return await _reservations.CountDocumentsAsync(
            StatusIs(status),
            cancellationToken: cancellationToken);
    }

    public async Task<long> CountApprovedFutureAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        // Count future approved reservations using authoritative and legacy status casing.
        var filter = StatusIs(ReservationStatus.APPROVED) &
                     Builders<EnergyReservation>.Filter.Gt(
                         reservation => reservation.ScheduledTime,
                         nowUtc);
        return await _reservations.CountDocumentsAsync(
            filter,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> HasActiveReservationsForStationAsync(
        string stationId,
        CancellationToken cancellationToken = default)
    {
        // Check pending and approved reservations for one validated station identifier.
        if (!ObjectId.TryParse(stationId, out _))
        {
            return false;
        }

        var builder = Builders<EnergyReservation>.Filter;
        var filter = builder.Eq(reservation => reservation.StationId, stationId) &
                     StatusIn(ReservationStatus.PENDING, ReservationStatus.APPROVED);
        return await _reservations.Find(filter).AnyAsync(cancellationToken);
    }

    public async Task<long> CountByStationAndStatusAsync(
        string stationId,
        ReservationStatus status,
        CancellationToken cancellationToken = default)
    {
        // Count a station's reservations while recognizing legacy status casing.
        if (!ObjectId.TryParse(stationId, out _))
        {
            return 0;
        }

        var builder = Builders<EnergyReservation>.Filter;
        var filter = builder.Eq(reservation => reservation.StationId, stationId) &
                     StatusIs(status);
        return await _reservations.CountDocumentsAsync(
            filter,
            cancellationToken: cancellationToken);
    }

    public async Task<long> CountApprovedFutureByStationAsync(
        string stationId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        // Count future approved reservations belonging to one assigned station.
        if (!ObjectId.TryParse(stationId, out _))
        {
            return 0;
        }

        var builder = Builders<EnergyReservation>.Filter;
        var filter = builder.Eq(reservation => reservation.StationId, stationId) &
                     StatusIs(ReservationStatus.APPROVED) &
                     builder.Gt(reservation => reservation.ScheduledTime, nowUtc);
        return await _reservations.CountDocumentsAsync(
            filter,
            cancellationToken: cancellationToken);
    }

    private async Task<EnergyReservation?> FindOneAndUpdateAsync(
        FilterDefinition<EnergyReservation> filter,
        UpdateDefinition<EnergyReservation> update,
        CancellationToken cancellationToken)
    {
        // Apply a conditional transition and return the updated reservation atomically.
        return await _reservations.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<EnergyReservation>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);
    }

    private static FilterDefinition<EnergyReservation> StatusIs(ReservationStatus status)
    {
        // Match both new uppercase values and legacy TitleCase values.
        return StatusIn(status);
    }

    private static FilterDefinition<EnergyReservation> StatusIn(params ReservationStatus[] statuses)
    {
        // Build a compatibility filter for authoritative and legacy status strings.
        var values = statuses
            .SelectMany(status => new[] { status.ToString(), ToLegacyStatus(status) })
            .Distinct(StringComparer.Ordinal)
            .Select(value => (BsonValue)value);
        return new BsonDocument(
            nameof(EnergyReservation.Status),
            new BsonDocument("$in", new BsonArray(values)));
    }

    private static string ToLegacyStatus(ReservationStatus status)
    {
        // Produce the TitleCase value used by the original reservation implementation.
        var value = status.ToString().ToLowerInvariant();
        return char.ToUpperInvariant(value[0]) + value[1..];
    }
}
