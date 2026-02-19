using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using vault_backend.Data;
using vault_backend.Models.DTOs.Auth;
using vault_backend.Models.DTOs.Users;

namespace vault_backend.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly MongoDbContext _db;

    public UsersController(MongoDbContext db)
    {
        _db = db;
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();
        var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
            return NotFound();

        var update = Builders<vault_backend.Models.Entities.User>.Update
            .Set(u => u.Username, request.Username)
            .Set(u => u.Avatar, request.Avatar);
        await _db.Users.UpdateOneAsync(u => u.Id == userId, update);

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = request.Username,
            Email = user.Email,
            Avatar = request.Avatar
        });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var filter = Builders<vault_backend.Models.Entities.User>.Filter.Or(
            Builders<vault_backend.Models.Entities.User>.Filter.Regex(u => u.Username, new MongoDB.Bson.BsonRegularExpression(q, "i")),
            Builders<vault_backend.Models.Entities.User>.Filter.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(q, "i")));
        var users = await _db.Users.Find(filter).ToListAsync();

        return Ok(users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Avatar = u.Avatar
        }));
    }
}
