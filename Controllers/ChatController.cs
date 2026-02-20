using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using vault_backend.Data;
using vault_backend.Hubs;
using vault_backend.Models.DTOs.Chat;
using vault_backend.Models.Entities;

namespace vault_backend.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(MongoDbContext db, IHubContext<ChatHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    [HttpGet("messages/{friendId}")]
    public async Task<IActionResult> GetMessages(string friendId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var filter = Builders<Message>.Filter.Or(
            Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.SenderId, userId),
                Builders<Message>.Filter.Eq(m => m.ReceiverId, friendId)),
            Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.SenderId, friendId),
                Builders<Message>.Filter.Eq(m => m.ReceiverId, userId)));
        var sort = Builders<Message>.Sort.Ascending(m => m.Timestamp);
        var messages = await _db.Messages.Find(filter).Sort(sort).ToListAsync();

        return Ok(messages.Select(m => new MessageResponse
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = m.SenderName,
            ReceiverId = m.ReceiverId,
            Text = m.Text,
            Timestamp = m.Timestamp,
            Type = m.Type ?? "text",
            Location = m.Location != null ? new Models.DTOs.Chat.LocationData { Lat = m.Location.Lat, Lng = m.Location.Lng } : null
        }));
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        var message = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = userId,
            SenderName = userName,
            ReceiverId = request.ReceiverId,
            Text = request.Text,
            Timestamp = request.Timestamp == default ? DateTime.UtcNow : request.Timestamp,
            Type = request.Type ?? "text",
            Location = request.Location != null ? new Models.Entities.MessageLocation { Lat = request.Location.Lat, Lng = request.Location.Lng } : null
        };

        await _db.Messages.InsertOneAsync(message);

        var response = new MessageResponse
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = message.SenderName,
            ReceiverId = message.ReceiverId,
            Text = message.Text,
            Timestamp = message.Timestamp,
            Type = message.Type ?? "text",
            Location = message.Location != null ? new Models.DTOs.Chat.LocationData { Lat = message.Location.Lat, Lng = message.Location.Lng } : null
        };

        await _hubContext.Clients.Group(request.ReceiverId).SendAsync("ReceiveMessage", response);
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveMessage", response);

        return Ok();
    }
}
