namespace vault_backend.Models.Entities;

public class FriendRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FromUserId { get; set; } = string.Empty;
    public string FromUsername { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
