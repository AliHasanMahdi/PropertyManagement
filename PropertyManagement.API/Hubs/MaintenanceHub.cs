using Microsoft.AspNetCore.SignalR;

namespace PropertyManagement.API.Hubs
{
    public class MaintenanceHub : Hub
    {
        public async Task JoinBoard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "MaintenanceBoard");
        }

        public async Task LeaveBoard()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "MaintenanceBoard");
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "MaintenanceBoard");
            await base.OnConnectedAsync();
        }
    }
}