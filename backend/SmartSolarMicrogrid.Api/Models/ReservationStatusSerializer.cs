// Reads legacy TitleCase reservation statuses and writes authoritative uppercase enum names.
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace SmartSolarMicrogrid.Api.Models;

public sealed class ReservationStatusSerializer : SerializerBase<ReservationStatus>
{
    public override ReservationStatus Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        // Parse both legacy TitleCase and current uppercase status strings.
        var value = context.Reader.ReadString();
        if (Enum.TryParse<ReservationStatus>(value, ignoreCase: true, out var status) &&
            Enum.IsDefined(status))
        {
            return status;
        }

        throw new FormatException($"Unsupported reservation status '{value}'.");
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        ReservationStatus value)
    {
        // Persist the exact authoritative uppercase status name.
        context.Writer.WriteString(value.ToString());
    }
}
