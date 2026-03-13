using Microsoft.AspNetCore.SignalR;

namespace CafeManagement.Hubs;

public class OrderHub : Hub
{
    public async Task JoinGroup(string storeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Store_{storeId}");
    }
}
