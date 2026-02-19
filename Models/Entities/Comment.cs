namespace vault_backend.Models.Entities;

public class Comment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MediaId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
