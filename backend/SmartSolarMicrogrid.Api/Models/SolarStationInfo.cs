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
