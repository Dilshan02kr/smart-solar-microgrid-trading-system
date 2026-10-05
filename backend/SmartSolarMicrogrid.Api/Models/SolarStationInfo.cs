/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: SolarStationInfo.cs
 * Component: Microgrid Node and Station Management
 * Component Owner: R A K Hansika (IT23140998)
 *
 * Purpose:
 * Represents a MongoDB-backed solar station or business-facing microgrid node.
 */
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolarMicrogrid.Api.Models;

public sealed class SolarStationInfo
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("locationName")]
    public required string LocationName { get; set; }

    [BsonElement("latitude")]
    public double Latitude { get; set; }

    [BsonElement("longitude")]
    public double Longitude { get; set; }

    [BsonElement("totalCapacityKw")]
    public double TotalCapacityKw { get; set; }

    [BsonElement("operationalSchedule")]
    public required string OperationalSchedule { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public StationStatus Status { get; set; } = StationStatus.ACTIVE;

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }
}
