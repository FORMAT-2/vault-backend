namespace vault_backend.Models.Entities;

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
