namespace vault_backend.Models.DTOs.Media;

public class AddCommentRequest
{
    public string PhotoId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
