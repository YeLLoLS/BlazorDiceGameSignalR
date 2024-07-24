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
            //await AddMessageToChat(username, "has joined the party!");
            await base.OnConnectedAsync();

            await SendPlayers();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string username = Users.FirstOrDefault(u => u.Key == Context.ConnectionId).Value;
            if (string.IsNullOrEmpty(username))
            {
                return;
            }
            Users.Remove(Context.ConnectionId);

            await Task.CompletedTask;
            //await AddMessageToChat(username, "has left the room!");
        }

        public async Task SendRoll(string user, int roll)
        {
            await Clients.All.SendAsync("ReceiveRoll", user, roll);
        }

        public async Task RollTheDice(string user)
        {
            Random random = new Random();
            int diceRoll = random.Next(1, 7);
            await SendRoll(user, diceRoll);
        }

        private async Task SendPlayers()
        {
            var players = Users.Values.ToList();
            await Clients.All.SendAsync("ReceivePlayers", players);
        }
    }
}
