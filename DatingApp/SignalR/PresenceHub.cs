using DatingApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker traker;

        public PresenceHub(PresenceTracker traker)
        {
            this.traker = traker;
        }
        public override async Task OnConnectedAsync()
        {
            var isOnline = await this.traker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            if(isOnline)
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

            await GetOnlineUsers();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await this.traker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
            if(isOffline)
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            await base.OnDisconnectedAsync(exception);
        }

        private async Task GetOnlineUsers()
        {
            var currentUsers = await this.traker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }


    }
}
