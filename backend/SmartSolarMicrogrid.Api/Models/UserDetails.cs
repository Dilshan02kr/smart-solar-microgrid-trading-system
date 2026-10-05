/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: UserDetails.cs
 * Component: Web User and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Represents a MongoDB-backed user account shared by all supported roles.
 */
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolarMicrogrid.Api.Models;

public sealed class UserDetails
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [BsonElement("firstName")]
    public required string FirstName { get; set; }

    [BsonElement("lastName")]
    public required string LastName { get; set; }

    [BsonElement("email")]
    public required string Email { get; set; }

    [BsonElement("phone")]
    public required string Phone { get; set; }

    [BsonElement("passwordHash")]
    public required string PasswordHash { get; set; }

    [BsonElement("role")]
    [BsonRepresentation(BsonType.String)]
    public UserRole Role { get; set; }

    [BsonElement("accountStatus")]
    [BsonRepresentation(BsonType.String)]
    public AccountStatus AccountStatus { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("nic")]
    [BsonIgnoreIfNull]
    public string? Nic { get; set; }

    [BsonElement("assignedMicrogridNodeId")]
    [BsonIgnoreIfNull]
    public ObjectId? AssignedMicrogridNodeId { get; set; }
}
