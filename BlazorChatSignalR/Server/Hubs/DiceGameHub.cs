using Microsoft.AspNetCore.SignalR;

namespace BlazorChatSignalR.Server.Hubs
{
    public class DiceGameHub : Hub
    {
        private static readonly Dictionary<string, string> Users = new Dictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            string username = Context.GetHttpContext().Request.Query["username"];
            if(string.IsNullOrEmpty(username))
            {
                Context.Abort();
                return;
            }
            Users.Add(Context.ConnectionId, username);
            await AddMessageToChat(username, "has joined the party!");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string username = Users.FirstOrDefault(u => u.Key == Context.ConnectionId).Value;
            if (string.IsNullOrEmpty(username))
            {
                return;
            }
            Users.Remove(Context.ConnectionId);
            await AddMessageToChat(username, "has left the room!");
        }

        public async Task AddMessageToChat(string user, string message)
        {
            await Clients.All.SendAsync("GetThatMessageDude", user, message);
        }
    }
}
