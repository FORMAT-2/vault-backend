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
            Timestamp = m.Timestamp
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
            Timestamp = request.Timestamp == default ? DateTime.UtcNow : request.Timestamp
        };

        await _db.Messages.InsertOneAsync(message);

        var response = new MessageResponse
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = message.SenderName,
            ReceiverId = message.ReceiverId,
            Text = message.Text,
            Timestamp = message.Timestamp
        };

        await _hubContext.Clients.Group(request.ReceiverId).SendAsync("ReceiveMessage", response);

        return Ok();
    }
}
