namespace BlazorChatSignalR.Shared
{
    public class Player
    {
        public Player(string name)
        {
            Name = name;
        }
        public string Name { get; set; }

        public int Roll { get; set; } = 0;
    }
}
