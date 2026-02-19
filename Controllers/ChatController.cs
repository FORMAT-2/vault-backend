using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
    private readonly AppDbContext _db;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(AppDbContext db, IHubContext<ChatHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    [HttpGet("messages/{friendId}")]
    public IActionResult GetMessages(string friendId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var messages = _db.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == friendId) ||
                        (m.SenderId == friendId && m.ReceiverId == userId))
            .OrderBy(m => m.Timestamp)
            .Select(m => new MessageResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = m.SenderName,
                ReceiverId = m.ReceiverId,
                Text = m.Text,
                Timestamp = m.Timestamp
            })
            .ToList();

        return Ok(messages);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var message = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = request.SenderId,
            SenderName = request.SenderName,
            ReceiverId = request.ReceiverId,
            Text = request.Text,
            Timestamp = request.Timestamp == default ? DateTime.UtcNow : request.Timestamp
        };

        _db.Messages.Add(message);
        _db.SaveChanges();

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
