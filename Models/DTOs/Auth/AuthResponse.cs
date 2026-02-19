namespace vault_backend.Models.DTOs.Auth;

public class AuthResponse
{
    public UserDto User { get; set; } = new();
    public string Token { get; set; } = string.Empty;
}
