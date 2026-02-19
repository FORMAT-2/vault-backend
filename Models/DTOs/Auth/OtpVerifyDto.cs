namespace vault_backend.Models.DTOs.Auth;

public class OtpVerifyDto
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}
