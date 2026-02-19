using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vault_backend.Data;
using vault_backend.Models.DTOs.Auth;
using vault_backend.Models.Entities;
using vault_backend.Services;

namespace vault_backend.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return BadRequest(new { message = "Invalid credentials" });

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            User = new UserDto { Id = user.Id, Username = user.Username, Email = user.Email, Avatar = user.Avatar },
            Token = token
        });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (_db.Users.Any(u => u.Email == request.Email))
            return BadRequest(new { message = "Email already exists" });

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Avatar = request.Avatar
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            User = new UserDto { Id = user.Id, Username = user.Username, Email = user.Email, Avatar = user.Avatar },
            Token = token
        });
    }

    [HttpPost("otp/request")]
    public IActionResult RequestOtp([FromBody] OtpRequestDto request)
    {
        var otp = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        _db.OtpRecords.Add(new OtpRecord
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            Otp = otp,
            CreatedAt = DateTime.UtcNow,
            IsVerified = false
        });
        _db.SaveChanges();
        return Ok(new { otp });
    }

    [HttpPost("otp/verify")]
    public IActionResult VerifyOtp([FromBody] OtpVerifyDto request)
    {
        var expiry = DateTime.UtcNow.AddMinutes(-10);
        var record = _db.OtpRecords.FirstOrDefault(o =>
            o.Email == request.Email && o.Otp == request.Otp &&
            !o.IsVerified && o.CreatedAt >= expiry);
        if (record == null)
            return BadRequest(new { message = "Invalid or expired OTP" });

        record.IsVerified = true;
        _db.SaveChanges();
        return Ok(new { success = true });
    }

    [HttpPost("password/reset")]
    public IActionResult ResetPassword([FromBody] ResetPasswordDto request)
    {
        var verifiedOtp = _db.OtpRecords.FirstOrDefault(o => o.Email == request.Email && o.IsVerified);
        if (verifiedOtp == null)
            return BadRequest(new { message = "OTP not verified" });

        var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
            return BadRequest(new { message = "User not found" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        var usedOtps = _db.OtpRecords.Where(o => o.Email == request.Email).ToList();
        _db.OtpRecords.RemoveRange(usedOtps);

        _db.SaveChanges();
        return Ok();
    }
}
