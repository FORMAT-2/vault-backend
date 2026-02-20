using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using vault_backend.Data;
using vault_backend.Models.DTOs.Auth;
using vault_backend.Models.DTOs.Social;
using vault_backend.Models.Entities;

namespace vault_backend.Controllers;

[ApiController]
[Route("api/social")]
[Authorize]
public class SocialController : ControllerBase
{
    private readonly MongoDbContext _db;

    public SocialController(MongoDbContext db)
    {
        _db = db;
    }

    [HttpPost("request/send")]
    public async Task<IActionResult> SendRequest([FromBody] SendFriendRequestDto request)
    {
        var fromUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var sender = await _db.Users.Find(u => u.Id == fromUserId).FirstOrDefaultAsync();
        if (sender == null)
            return BadRequest(new { message = "Sender not found" });

        var existing = await _db.FriendRequests.Find(r =>
            ((r.FromUserId == fromUserId && r.ToUserId == request.ToUserId) ||
             (r.FromUserId == request.ToUserId && r.ToUserId == fromUserId)) &&
            (r.Status == "pending" || r.Status == "accepted")).FirstOrDefaultAsync();
        if (existing != null)
            return Conflict(new { message = "Friend request already exists" });

        await _db.FriendRequests.InsertOneAsync(new FriendRequest
        {
            Id = Guid.NewGuid().ToString(),
            FromUserId = fromUserId,
            FromUsername = sender.Username,
            ToUserId = request.ToUserId,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        });
        return Ok();
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var requests = await _db.FriendRequests.Find(r => r.ToUserId == userId).ToListAsync();
        return Ok(requests.Select(r => new FriendRequestResponse
        {
            Id = r.Id,
            FromUserId = r.FromUserId,
            FromUsername = r.FromUsername,
            ToUserId = r.ToUserId,
            Status = r.Status,
            CreatedAt = r.CreatedAt
        }));
    }

    [HttpPost("request/respond")]
    public async Task<IActionResult> RespondRequest([FromBody] RespondFriendRequestDto request)
    {
        if (request.Status != "accepted" && request.Status != "rejected")
            return BadRequest(new { message = "Status must be 'accepted' or 'rejected'" });

        var fr = await _db.FriendRequests.Find(r => r.Id == request.RequestId).FirstOrDefaultAsync();
        if (fr == null)
            return NotFound();

        var update = Builders<FriendRequest>.Update.Set(r => r.Status, request.Status);
        await _db.FriendRequests.UpdateOneAsync(r => r.Id == request.RequestId, update);
        return Ok();
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var filter = Builders<FriendRequest>.Filter.And(
            Builders<FriendRequest>.Filter.Eq(r => r.Status, "accepted"),
            Builders<FriendRequest>.Filter.Or(
                Builders<FriendRequest>.Filter.Eq(r => r.FromUserId, userId),
                Builders<FriendRequest>.Filter.Eq(r => r.ToUserId, userId)));
        var accepted = await _db.FriendRequests.Find(filter).ToListAsync();

        var otherIds = accepted
            .Select(r => r.FromUserId == userId ? r.ToUserId : r.FromUserId)
            .Distinct()
            .ToList();

        var userFilter = Builders<vault_backend.Models.Entities.User>.Filter.In(u => u.Id, otherIds);
        var users = await _db.Users.Find(userFilter).ToListAsync();

        return Ok(users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Avatar = u.Avatar
        }));
    }

    [HttpPost("partner/set")]
    public async Task<IActionResult> SetPartner([FromBody] SetPartnerRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var update = Builders<vault_backend.Models.Entities.User>.Update.Set(u => u.PartnerId, request.PartnerId);
        await _db.Users.UpdateOneAsync(u => u.Id == userId, update);
        return Ok();
    }

    [HttpGet("partner")]
    public async Task<IActionResult> GetPartner()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
            return NotFound();

        return Ok(new GetPartnerResponse { PartnerId = user.PartnerId });
    }

    [HttpDelete("partner/remove")]
    public async Task<IActionResult> RemovePartner()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var update = Builders<vault_backend.Models.Entities.User>.Update.Set(u => u.PartnerId, null);
        await _db.Users.UpdateOneAsync(u => u.Id == userId, update);
        return Ok();
    }
}
