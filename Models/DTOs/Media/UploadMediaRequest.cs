namespace vault_backend.Models.DTOs.Media;

public class UploadMediaRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string MediaData { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
}
