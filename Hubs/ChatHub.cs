using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using vault_backend.Data;

namespace vault_backend.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly MongoDbContext _db;

    public ChatHub(MongoDbContext db)
    {
        _db = db;
    }

    // Automatically adds connected user to their own "user group"
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier; // from JWT claim
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
    }

    // Send message to a specific user securely
    public async Task SendMessage(string receiverId, object message)
    {
        await Clients.Group(receiverId).SendAsync("ReceiveMessage", message);
    }

    // WebRTC Signaling: Initiate a call
    public async Task INITIATE_CALL(JsonElement data)
    {
        if (!data.TryGetProperty("to", out var toProp) || string.IsNullOrEmpty(toProp.GetString()))
            return;

        var to = toProp.GetString()!;
        var callerId = Context.UserIdentifier;

        await Clients.Group(to).SendAsync("INCOMING_CALL", new
        {
            offer = data.GetProperty("offer"),
            callType = data.TryGetProperty("callType", out var ct) ? ct.GetString() : "audio",
            fromId = callerId,
            from = callerId
        });
    }

    // WebRTC Signaling: Answer a call
    public async Task ANSWER_CALL(JsonElement data)
    {
        if (!data.TryGetProperty("to", out var toProp) || string.IsNullOrEmpty(toProp.GetString()))
            return;

        var to = toProp.GetString()!;

        await Clients.Group(to).SendAsync("CALL_ANSWER", new
        {
            answer = data.GetProperty("answer")
        });
    }

    // WebRTC Signaling: Exchange ICE candidates
    public async Task SEND_ICE_CANDIDATE(JsonElement data)
    {
        if (!data.TryGetProperty("to", out var toProp) || string.IsNullOrEmpty(toProp.GetString()))
            return;

        var to = toProp.GetString()!;

        await Clients.Group(to).SendAsync("ICE_CANDIDATE", new
        {
            candidate = data.GetProperty("candidate")
        });
    }

    // WebRTC Signaling: End a call
    public async Task END_CALL(JsonElement data)
    {
        if (!data.TryGetProperty("to", out var toProp) || string.IsNullOrEmpty(toProp.GetString()))
            return;

        var to = toProp.GetString()!;

        await Clients.Group(to).SendAsync("END_CALL");
    }

    // Miss You Signal (Vibration)
    public async Task MISS_YOU_SIGNAL(JsonElement data)
    {
        if (!data.TryGetProperty("to", out var toProp) || string.IsNullOrEmpty(toProp.GetString()))
            return;

        var to = toProp.GetString()!;
        var from = Context.UserIdentifier;
        var message = data.TryGetProperty("message", out var msgProp) ? msgProp.GetString() ?? "" : "";

        await Clients.Group(to).SendAsync("MISS_YOU_SIGNAL", new
        {
            from = from,
            message = message
        });
    }

    // Real-time Location Update - sends only to the user's partner
    public async Task UPDATE_LOCATION(JsonElement data)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
            return;

        // Look up the user's partner and send only to them
        var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user?.PartnerId != null)
        {
            await Clients.Group(user.PartnerId).SendAsync("PARTNER_LOCATION_UPDATE", (object)data);
        }
    }

    // SOS Alert (Real-time)
    public async Task SOS_ALERT(JsonElement data)
    {
        if (!data.TryGetProperty("to", out var toProp) || string.IsNullOrEmpty(toProp.GetString()))
            return;

        var to = toProp.GetString()!;
        var message = data.TryGetProperty("message", out var msgProp) ? msgProp.GetString() ?? "" : "";

        await Clients.Group(to).SendAsync("SOS_ALERT", new
        {
            from = Context.UserIdentifier,
            location = data.TryGetProperty("location", out var loc) ? (object)loc : null,
            message = message
        });
    }
}
