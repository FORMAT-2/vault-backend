namespace vault_backend.Models.DTOs.Chat;

public class MessageResponse
{
    public string Id { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Type { get; set; }
    public LocationData? Location { get; set; }
}
