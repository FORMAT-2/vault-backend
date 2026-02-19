namespace vault_backend.Models.DTOs.Media;

public class MediaResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public List<CommentResponse> Comments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public int Likes { get; set; }
}
