namespace vault_backend.Models.Entities;

public class Media
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public List<Comment> Comments { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int Likes { get; set; }
}
