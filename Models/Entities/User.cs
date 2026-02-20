using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace vault_backend.Models.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? PartnerId { get; set; }
}
