namespace vault_backend.Models.DTOs.Users;

public class UpdateProfileRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}
