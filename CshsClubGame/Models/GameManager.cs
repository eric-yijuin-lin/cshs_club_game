namespace CshsClubGame.Models
{
    public class GameManager
    {
        private readonly Dictionary<string, Player> _players;
        private readonly Dictionary<string, GameRoom> _rooms;
        private readonly LootHelper _lootHelper;
        private readonly CardHelper _cardHelper;

        public GameManager(LootHelper lootHelper, CardHelper cardHelper)
        {
            _players = new Dictionary<string, Player>();
            _rooms = new Dictionary<string, GameRoom>();
            _lootHelper = lootHelper;
            _cardHelper = cardHelper;
        }

        private void DeletePlayer(Player player)
        {
            _players.Remove(player.Id);
            this.MovePlayerOutFromRoom(player);
        }

        private void MovePlayerOutFromRoom(Player player)
        {
            foreach (var kvp in _rooms)
            {
                if (kvp.Value.IsPlayerInRoom(player.Id))
                {
                    kvp.Value.RemovePlayer(player);
                }
            }
            player.RoomId = null;
        }

        public GameRoom CreateRoom(string roomName, int maxPlayer)
        {
            var guid = Guid.NewGuid().ToString();
            var room = new GameRoom(guid, roomName, maxPlayer);
            _rooms[guid] = room;
            return room;
        }

        public Player CreatePlayer(string classUnit, string seatNo, string name)
        {
            var player = new Player(classUnit, seatNo, name);
            _players[player.Id] = player;
            return player;
        }
#if DEBUG
        public GameRoom CreateDebugRoom()
        {
            var room = new GameRoom("r01", "debug room", 5);
            room.Id = "r1";
            _rooms[room.Id] = room;
            return room;
        }

        public Player CreateDebugPlayer(int index)
        {
            var player = new Player("debug", index.ToString().PadLeft(2, '0'), $"debugger-{index}");
            player.Id = "p" + index;
            _players[player.Id] = player;
            return player;
        }
#endif

        public string? GetRoomIdByPlayerId(string playerId)
        {
            foreach (var kvp in _rooms)
            {
                if (kvp.Value.IsPlayerInRoom(playerId))
                {
                    return kvp.Value.Id;
                }
            }
            return null;
        }

        public GameRoom? JoinRoom(string roomId, Player player)
        {
            var room = _rooms[roomId];
            if (room.IsPlayerInRoom(player.Id))
            {
                return room;
            }

            this.MovePlayerOutFromRoom(player);
            if (room.AddPlayer(player))
            {
                player.RoomId = roomId;
                return room;
            };
            return null;
        }

        public Player? GetPlayer(string playerId)
        {
            return _players[playerId];
        }

        public List<GameCard> GetTurnCards(string playerId)
        {
            return _cardHelper.GetTurnCards(playerId, _players);
        }

        public string ValidateBattle(string roomId, Player? player, Player? target)
        {
            if (player == null || target == null)
            {
                return "找不到玩家";
            }
            var room = _rooms[roomId];
            if (!room.IsPlayersInSameRoom(player, target))
            {
                return "玩家不在同一間房";
            }
            if (player.Hp <= 0)
            {
                return "你已經死了";
            }
            if (target.Hp <= 0)
            {
                return "目標已經死了";
            }
            return "OK";
        }

        public BattleRecord ProcessBattle(Player player, Player target)
        {
            player.Hp -= target.Atk;
            target.Hp -= player.Atk;
            var battleResult = new BattleRecord()
            {
                SelfHp = player.Hp,
                TargetHp = target.Hp,
                BattleTime = DateTime.Now
            };
            if (player.Hp <= 0)
            {
                this.DeletePlayer(player);
                return battleResult;
            }
            if (target.Hp <= 0)
            {
                this.DeletePlayer(target);
                battleResult.LootExp = _lootHelper.GetLootExp(player, target);
                battleResult.LootExpEquipment = _lootHelper.GetLootEquipment(target);
            }
            return battleResult;
        }
    }
}
