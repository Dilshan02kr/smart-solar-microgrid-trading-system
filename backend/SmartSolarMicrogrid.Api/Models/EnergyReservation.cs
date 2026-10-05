/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: EnergyReservation.cs
 * Component: Reservation and Booking Management
 * Component Owner: N A Illangasinghe (IT23391536)
 *
 * Purpose:
 * Represents a server-controlled reservation persisted in MongoDB.
 */
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolarMicrogrid.Api.Models;

public sealed class EnergyReservation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public required string ProsumerId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string StationId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string SlotId { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ScheduledTime { get; set; }

    [BsonSerializer(typeof(ReservationStatusSerializer))]
    public ReservationStatus Status { get; set; } = ReservationStatus.PENDING;

    [BsonIgnoreIfNull]
    public string? TransactionReference { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }

    [BsonIgnoreIfNull]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? CompletedAt { get; set; }
}
