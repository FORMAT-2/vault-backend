using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace vault_backend.Models.Entities;

public class Like
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MediaId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
