using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;


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
}
