using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace vault_backend.Hubs;

[Authorize]
public class ChatHub : Hub
{
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
        var to = data.GetProperty("to").GetString()!;
        var from = data.GetProperty("from").GetString()!;
        var callerId = Context.UserIdentifier;

        await Clients.Group(to).SendAsync("INCOMING_CALL", new
        {
            offer = data.GetProperty("offer"),
            callType = data.GetProperty("callType").GetString(),
            fromId = callerId,
            from = from
        });
    }

    // WebRTC Signaling: Answer a call
    public async Task ANSWER_CALL(JsonElement data)
    {
        var to = data.GetProperty("to").GetString()!;

        await Clients.Group(to).SendAsync("CALL_ANSWER", new
        {
            answer = data.GetProperty("answer")
        });
    }

    // WebRTC Signaling: Exchange ICE candidates
    public async Task SEND_ICE_CANDIDATE(JsonElement data)
    {
        var to = data.GetProperty("to").GetString()!;

        await Clients.Group(to).SendAsync("ICE_CANDIDATE", new
        {
            candidate = data.GetProperty("candidate")
        });
    }

    // WebRTC Signaling: End a call
    public async Task END_CALL(JsonElement data)
    {
        var to = data.GetProperty("to").GetString()!;

        await Clients.Group(to).SendAsync("END_CALL");
    }

    // Miss You Signal (Vibration)
    public async Task MISS_YOU_SIGNAL(JsonElement data)
    {
        var to = data.GetProperty("to").GetString()!;
        var from = data.GetProperty("from").GetString()!;
        var message = data.GetProperty("message").GetString()!;

        await Clients.Group(to).SendAsync("MISS_YOU_SIGNAL", new
        {
            from = from,
            message = message
        });
    }

    // Real-time Location Update
    public async Task UPDATE_LOCATION(JsonElement data)
    {
        var userId = Context.UserIdentifier;
        // Broadcast location update to all other connected clients
        // The partner filtering is handled client-side
        await Clients.Others.SendAsync("PARTNER_LOCATION_UPDATE", (object)data);
    }

    // SOS Alert (Real-time)
    public async Task SOS_ALERT(JsonElement data)
    {
        var to = data.GetProperty("to").GetString()!;
        var message = data.GetProperty("message").GetString()!;

        await Clients.Group(to).SendAsync("SOS_ALERT", new
        {
            from = Context.UserIdentifier,
            location = data.GetProperty("location"),
            message = message
        });
    }
}
