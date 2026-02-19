using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly AppDbContext _db;

    public SocialController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("request/send")]
    public IActionResult SendRequest([FromBody] SendFriendRequestDto request)
    {
        var fromUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var sender = _db.Users.FirstOrDefault(u => u.Id == fromUserId);
        if (sender == null)
            return BadRequest(new { message = "Sender not found" });

        _db.FriendRequests.Add(new FriendRequest
        {
            Id = Guid.NewGuid().ToString(),
            FromUserId = fromUserId,
            FromUsername = sender.Username,
            ToUserId = request.ToUserId,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        });
        _db.SaveChanges();
        return Ok();
    }

    [HttpGet("requests")]
    public IActionResult GetRequests()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var requests = _db.FriendRequests
            .Where(r => r.ToUserId == userId)
            .Select(r => new FriendRequestResponse
            {
                Id = r.Id,
                FromUserId = r.FromUserId,
                FromUsername = r.FromUsername,
                ToUserId = r.ToUserId,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToList();
        return Ok(requests);
    }

    [HttpPost("request/respond")]
    public IActionResult RespondRequest([FromBody] RespondFriendRequestDto request)
    {
        var fr = _db.FriendRequests.FirstOrDefault(r => r.Id == request.RequestId);
        if (fr == null)
            return NotFound();

        fr.Status = request.Status;
        _db.SaveChanges();
        return Ok();
    }

    [HttpGet("friends")]
    public IActionResult GetFriends()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var accepted = _db.FriendRequests
            .Where(r => r.Status == "accepted" && (r.FromUserId == userId || r.ToUserId == userId))
            .ToList();

        var otherIds = accepted
            .Select(r => r.FromUserId == userId ? r.ToUserId : r.FromUserId)
            .Distinct()
            .ToList();

        var users = _db.Users
            .Where(u => otherIds.Contains(u.Id))
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Avatar = u.Avatar
            })
            .ToList();

        return Ok(users);
    }
}
