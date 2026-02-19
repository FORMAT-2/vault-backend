namespace vault_backend.Models.DTOs.Media;

public class LikeMediaRequest
{
    public string PhotoId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
