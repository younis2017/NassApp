using Microsoft.AspNetCore.SignalR;

namespace Nass.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var tenant = Context.GetHttpContext()?.Request.Query["tenant"];
            if (!string.IsNullOrEmpty(tenant))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"Tenant-{tenant}"
                );
            }

            await base.OnConnectedAsync();
        }
    }
}
