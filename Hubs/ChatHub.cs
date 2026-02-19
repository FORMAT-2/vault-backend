using Microsoft.AspNetCore.SignalR;

namespace vault_backend.Hubs;

public class ChatHub : Hub
{
    public async Task JoinChat(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

    public async Task SendMessage(string receiverId, object message)
    {
        await Clients.Group(receiverId).SendAsync("ReceiveMessage", message);
    }
}
