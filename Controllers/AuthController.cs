using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StackExchange.Redis;
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
    private readonly MongoDbContext _db;
    private readonly TokenService _tokenService;
    private readonly IConnectionMultiplexer _redis;
    private readonly EmailService _emailService;

    public AuthController(MongoDbContext db, TokenService tokenService, IConnectionMultiplexer redis, EmailService emailService)
    {
        _db = db;
        _tokenService = tokenService;
        _redis = redis;
        _emailService = emailService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
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
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var exists = await _db.Users.Find(u => u.Email == request.Email).AnyAsync();
        if (exists)
            return BadRequest(new { message = "Email already exists" });

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Avatar = request.Avatar
        };

        await _db.Users.InsertOneAsync(user);

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            User = new UserDto { Id = user.Id, Username = user.Username, Email = user.Email, Avatar = user.Avatar },
            Token = token
        });
    }

    [HttpPost("otp/request")]
    public async Task<IActionResult> RequestOtp([FromBody] OtpRequestDto request)
    {
        var otp = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var db = _redis.GetDatabase();
        var key = $"otp:{request.Email}";
        await db.StringSetAsync(key, otp, TimeSpan.FromMinutes(10));

        await _emailService.SendOtpEmailAsync(request.Email, otp);
        return Ok(new { message = "If the email is registered, an OTP has been sent." });
    }

    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyDto request)
    {
        var db = _redis.GetDatabase();
        var key = $"otp:{request.Email}";
        var storedOtp = await db.StringGetAsync(key);
        if (storedOtp.IsNullOrEmpty || storedOtp != request.Otp)
            return BadRequest(new { message = "Invalid or expired OTP" });

        await db.StringSetAsync($"otp:verified:{request.Email}", "1", TimeSpan.FromMinutes(15));
        await db.KeyDeleteAsync(key);
        return Ok(new { success = true });
    }

    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var db = _redis.GetDatabase();
        var verified = await db.StringGetAsync($"otp:verified:{request.Email}");
        if (verified.IsNullOrEmpty)
            return BadRequest(new { message = "OTP not verified" });

        var user = await _db.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (user == null)
            return BadRequest(new { message = "User not found" });

        var update = Builders<User>.Update.Set(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await _db.Users.UpdateOneAsync(u => u.Id == user.Id, update);

        await db.KeyDeleteAsync($"otp:verified:{request.Email}");
        return Ok();
    }
}
