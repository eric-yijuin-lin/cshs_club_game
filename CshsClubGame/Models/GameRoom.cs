namespace CshsClubGame.Models
{
    public class GameRoom
    {
        private readonly Dictionary<string, Player> _players;
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int MaxPlayerCount { get; }
        public int CurrentPlayerCount { get { return _players.Count; } }

        public GameRoom(string roomId, string roomName, int maxPlayer)
        {
            _players = new Dictionary<string, Player>();
            this.Id = roomId;
            this.Name = roomName;
            this.MaxPlayerCount = maxPlayer;
        }

        public bool AddPlayer(Player player)
        {
            if (_players.Count >= this.MaxPlayerCount)
            {
                return false;
            }
            _players[player.Id] = player;
            return true;
        }

        public void RemovePlayer(Player player)
        {
            if (_players.ContainsKey(player.Id))
            {
                _players.Remove(player.Id);
            }
        }

        public Player GetPlayer(string playerId)
        {
            return _players[playerId];
        }

        public bool IsPlayerInRoom(string playerId)
        {
            return _players.ContainsKey(playerId);
        }

        public bool IsPlayersInSameRoom(Player self, Player target)
        {
            if (string.IsNullOrEmpty(self.RoomId))
            {
                return false;
            }
            if (target.IsNpc)
            {
                return true;
            }
            if (self.RoomId != target.RoomId)
            {
                return false;
            }
            return true;
        }
    }
}
