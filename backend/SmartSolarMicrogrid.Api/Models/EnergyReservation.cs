using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolarMicrogrid.Api.Models
{
    public class EnergyReservation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string ProsumerId { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string SlotId { get; set; } = string.Empty;

        public DateTime ScheduledTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Statuses: "Pending", "Approved", "Completed", "Cancelled"
        public string Status { get; set; } = "Pending";

        // Transaction reference for Member 4's QR scanning workflow
        public string TransactionReference { get; set; } = Guid.NewGuid().ToString("N");
    }
}