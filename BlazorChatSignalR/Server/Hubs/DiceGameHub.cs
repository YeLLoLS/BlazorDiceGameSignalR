using BlazorChatSignalR.Shared;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChatSignalR.Server.Hubs
{
    public class DiceGameHub : Hub
    {
        private static readonly Dictionary<string, Player> Users = new Dictionary<string, Player>();

        public override async Task OnConnectedAsync()
        {
            string username = Context.GetHttpContext().Request.Query["username"];
            if(string.IsNullOrEmpty(username))
            {
                Context.Abort();
                return;
            }
            Users.Add(Context.ConnectionId, new Player(username));
            await SendPlayers();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string username = Users.FirstOrDefault(u => u.Key == Context.ConnectionId).Value.Name;
            if (string.IsNullOrEmpty(username))
            {
                return;
            }
            Users.Remove(Context.ConnectionId);
            await SendPlayers();
            await Task.CompletedTask;
        }

        public async Task RollTheDice()
        {
            Random random = new Random();
            int diceRoll = random.Next(1, 7);
            await SetRoll(diceRoll);
            await SendRoll();
        }

        private async Task SendRoll()
        {
            var players = Users.Values.ToList();
            await Clients.All.SendAsync("ReceiveRoll", players);
            await DecideWinner();
        }

        private async Task SendPlayers()
        {
            var players = Users.Values.ToList();
            await Clients.All.SendAsync("ReceivePlayers", players);
        }

        private async Task SetRoll(int roll)
        {
            Users.FirstOrDefault(u => u.Key == Context.ConnectionId).Value.Roll = roll;
            await Task.CompletedTask;
        }

        private async Task DecideWinner()
        {
            if(Users.Count < 2)
            {
                return;
            }

            if(Users.Values.All(p => p.Roll == 0))
            {
                return;
            }

            if(Users.Values.All(p => p.Roll > 0))
            {
                var players = Users.Values.ToList();
                var winner = players.OrderByDescending(p => p.Roll).First();
                await Clients.All.SendAsync("ReceiveWinner", winner);
                await ResetRolls();
            }
        }

        private async Task ResetRolls()
        {
            foreach(var user in Users)
            {
                user.Value.Roll = 0;
            }

            var players = Users.Values.ToList();
            await Clients.All.SendAsync("ReceiveRollsReset", players);
            await Task.CompletedTask;
        }
    }
}
