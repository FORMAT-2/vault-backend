namespace vault_backend.Models.DTOs.Chat;

public class SendMessageRequest
{
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Type { get; set; } = "text";
    public LocationData? Location { get; set; }
}

public class LocationData
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}
