using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace vault_backend.Models.Entities;

public class Message
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Type { get; set; } = "text";
    public MessageLocation? Location { get; set; }
}

public class MessageLocation
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}
