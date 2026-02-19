using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vault_backend.Data;
using vault_backend.Models.DTOs.Auth;
using vault_backend.Models.DTOs.Users;

namespace vault_backend.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPut("profile")]
    public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();
        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return NotFound();

        user.Username = request.Username;
        user.Avatar = request.Avatar;
        _db.SaveChanges();

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Avatar = user.Avatar
        });
    }

    [HttpGet("search")]
    public IActionResult Search([FromQuery] string q)
    {
        var lower = q.ToLower();
        var results = _db.Users
            .Where(u => u.Username.ToLower().Contains(lower) || u.Email.ToLower().Contains(lower))
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Avatar = u.Avatar
            })
            .ToList();

        return Ok(results);
    }
}
