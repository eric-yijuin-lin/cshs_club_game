using System.Text.Json;
using System.Text.Json.Nodes;

namespace CshsClubGame.Models
{
    public class GameManager
    {
        public const string LOBBY_ID = "e6553cf38154794e9a4fb7e3162b9f83"; // MD5 of "publicLobby" 
        public const string GRAVEYARD_ID = "7e0661d958193467fa8680a1216f4d57"; // MD5 of "R.I.P" 
        private readonly Dictionary<string, Player> _players;
        private readonly Dictionary<string, GameRoom> _rooms;
        private readonly LootHelper _lootHelper;
        private readonly CardHelper _cardHelper;
        private readonly GameHistoryHelper _historyHelper;

        public GameManager(LootHelper lootHelper, CardHelper cardHelper, GameHistoryHelper historyHelper)
        {
            _players = new Dictionary<string, Player>();
            _rooms = new Dictionary<string, GameRoom>();
            _rooms.Add(LOBBY_ID, new GameRoom(LOBBY_ID, "大廳", 100));
            _rooms.Add(GRAVEYARD_ID, new GameRoom(GRAVEYARD_ID, "墓地", int.MaxValue));
            _lootHelper = lootHelper;
            _cardHelper = cardHelper;
            _historyHelper = historyHelper;
        }

        public void Rest()
        {
            _rooms.Clear();
            _players.Clear();
            _historyHelper.ClearHistory();
            _rooms.Add(LOBBY_ID, new GameRoom(LOBBY_ID, "大廳", 100));
        }

        public GameRoom CreateRoom(string roomName, int maxPlayer)
        {
            var guid = Guid.NewGuid().ToString();
            var room = new GameRoom(guid, roomName, maxPlayer);
            _rooms[guid] = room;
            return room;
        }

        public Player CreatePlayer(string classUnit, string name)
        {
            var player = new Player(classUnit, name);
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
            var player = new Player("debug", $"debugger-{index}");
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
            if (roomId == "restricted")
            {
                roomId = LOBBY_ID;
            }

            var room = _rooms[roomId]; // 暫時改成只能加大廳
            if (room.IsPlayerInRoom(player.Id))
            {
                return room;
            }

            this.MovePlayerOutFromRoom(player);
            if (room.AddPlayer(player))
            {
                player.RoomId = roomId;
                _historyHelper.AddJoinRoomHistory(player);
                return room;
            };
            return null;
        }

        public Player GetPlayerById(string playerId)
        {
            return _players[playerId];
        }

        public Player? GetPlayerByCard(CharaterCard? card)
        {
            if (card == null)
            {
                return null;
            }
            if (card.Id == "npc")
            {
                return new Player(card);
            }
            else
            {
                return this.GetPlayerById(card.Id);
            }
        }

        public List<GameCard> GetTurnCard(string playerId)
        {
            return _cardHelper.GetTurnCards(playerId, _players);
        }

        public TurnRecord ProcessTurnCard(string selfId, JsonObject card)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            CardType cardType = (CardType)card["cardType"]!.GetValue<int>();
            switch (cardType)
            {
                case CardType.Character:
                case CardType.Npc:
                    var charaterCard = card.Deserialize<CharaterCard>(options);
                    return this.ProcessBattleCard(selfId, charaterCard);
                case CardType.Equipment:
                    var equipmentCard = card.Deserialize<EquipmentCard>(options);
                    return this.ProcessEquipmentCard(selfId, equipmentCard);
                case CardType.Event:
                    var eventCard = card.Deserialize<EventCard>(options);
                    return this.ProcessEventCard(selfId, eventCard);
                default:
                    return new TurnRecord(TurnType.Undefined)
                    {
                        Status = TurnStatus.Error,
                        Message = $"無法辨識的卡片 enum：{cardType}"
                    };
            }
        }

        public GameHistoryEntry[] GetHistoryPage()
        {
            var historyPage = _historyHelper.GetHistoryPage(DateTime.Now);
            return historyPage;
        }

        private void DeletePlayer(Player player)
        {
            _players.Remove(player.Id);
            this.MovePlayerOutFromRoom(player);
        }

        private void MovePlayerToGrave(Player player)
        {
            this.MovePlayerOutFromRoom(player);
            _rooms[GRAVEYARD_ID].AddPlayer(player);
            player.Status = PlayerStatus.InGrave;
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

        private TurnRecord ProcessBattleCard(string selfId, CharaterCard? charaterCard)
        {
            var turnRecord = new TurnRecord(TurnType.Battle);
            string validateResult = this.ValidateCard(selfId, charaterCard);
            if (validateResult != "OK")
            {
                turnRecord.Message = validateResult;
                return turnRecord;
            }

            Player me = this.GetPlayerById(selfId);
            if (me.Status != PlayerStatus.Alive)// 先硬幹，之後再重構
            {
                turnRecord.Status = TurnStatus.Interrupted;
                turnRecord.Message = "你在發動攻擊前被暗算了";
                return turnRecord;
            }

            Player? target = this.GetPlayerByCard(charaterCard);
            validateResult = this.ValidateBattle(me, target);
            if (validateResult != "OK")
            {
                turnRecord.Message = validateResult;
                return turnRecord;
            }

            try
            {
                var battleRecord = this.ProcessBattle(me!, target!);
                me!.SurvivedDay += 1;
                _historyHelper.AddBattleHistory(me!, target!, battleRecord);
                turnRecord.PlayerMe = me;
                turnRecord.BattleRecord = battleRecord;
            }
            catch (Exception ex)
            {
                turnRecord.Status = TurnStatus.Error;
                turnRecord.Message = ex.Message;
                Console.WriteLine($"Server Error: [{DateTime.Now.ToString("HH:mm:ss")}] {ex.Message}");
                return turnRecord;
            }

            turnRecord.Status = TurnStatus.Ok;
            return turnRecord;
        }

        private string ValidateCard(string selfId, CharaterCard? charaterCard)
        {
            if (charaterCard == null || string.IsNullOrEmpty(charaterCard.Id))
            {
                return "對手 ID 不可為空";
            }
            string? roomId = this.GetRoomIdByPlayerId(selfId);
            if (string.IsNullOrEmpty(roomId))
            {
                return "找不到玩家的房間";
            }
            return "OK";
        }

        private string ValidateBattle(Player? player, Player? target)
        {
            if (player == null) return $"找不到玩家";
            if (target == null) return $"找不到目標";
            if (player.Status == PlayerStatus.InGrave) return "你在發動攻擊前被暗算了";
            if (target.Status == PlayerStatus.InGrave) return "目標墳墓上的草已經跟你一樣高了";
            var room = _rooms[player.RoomId];
            if (!room.IsPlayersInSameRoom(player, target))
            {
                return "玩家不在同一間房";
            }
            return "OK";
        }

        private BattleRecord ProcessBattle(Player self, Player target)
        {
            self.Hp -= target.Atk;
            self.SurvivedDay += 1;
            target.Hp -= self.Atk;
            var battleResult = new BattleRecord()
            {
                SelfHp = self.Hp,
                TargetHp = target.Hp,
                BattleTime = DateTime.Now
            };
            
            if (self.Hp > 0 && target.Hp <= 0)
            {
                int exp = _lootHelper.GetLootExp(self, target);
                int rankScore = _lootHelper.GetLootRankScore(self, target);
                var equipment = _lootHelper.GetLootEquipment(target);
                self.AddExp(exp);
                self.Rank += rankScore;
                self.AddEquipment(equipment);
                battleResult.Status = BattleStatus.MeWin;
                battleResult.LootExp = exp;
                battleResult.LootRankScore += rankScore;
                battleResult.LootExpEquipment = equipment;
                this.MovePlayerToGrave(target);
            }
            else if (self.Hp > 0 && target.Hp > 0)
            {
                battleResult.Status = BattleStatus.Draw;
            }
            else if (self.Hp <= 0)
            {
                battleResult.Status = BattleStatus.TargetWin;
                this.MovePlayerToGrave(self);
            }
            else if (target.Hp <= 0)
            {
                battleResult.Status = BattleStatus.MeWin;
                this.MovePlayerToGrave(target);
            }
            return battleResult;
        }

        private TurnRecord ProcessEquipmentCard(string selfId, EquipmentCard? equipmentCard)
        {
            var player = this.GetPlayerById(selfId);
            this.ValidateEquipmentTurn(player, equipmentCard);

            var equip = equipmentCard!.ConvertToEquipment();
            player!.AddEquipment(equip);
            player!.SurvivedDay += 1;

            _historyHelper.AddEquipHistory(player, equip);
            return new TurnRecord(TurnType.Equipment)
            {
                BattleRecord = null,
                PlayerMe = player,
                Status = TurnStatus.Ok,
            };
        }

        private void ValidateEquipmentTurn(Player? player, EquipmentCard? equipmentCard)
        {
            if (player == null)
            {
                throw new InvalidDataException("驗證裝備卡失敗：找不到人物");
            }
            if (equipmentCard == null)
            {
                throw new InvalidDataException("驗證裝備卡失敗：裝備不可為空");
            }
            if (string.IsNullOrEmpty(equipmentCard.Title)
                || (equipmentCard.EnhancedHp == 0 && equipmentCard.EnhancedAtk == 0))
            {
                throw new InvalidDataException("驗證裝備卡失敗：無效的裝備屬性");
            }
        }

        private TurnRecord ProcessEventCard(string selfId, EventCard? eventCard)
        {
            var me = this.GetPlayerById(selfId);
            this.ValidateEventTurn(me, eventCard);
            me!.Rest(eventCard!);
            me!.SurvivedDay += 1;

            _historyHelper.AddEventHistory(me, eventCard!);
            return new TurnRecord(TurnType.Event)
            {
                BattleRecord = null,
                PlayerMe = me,
                Status = TurnStatus.Ok,
            };
        }

        private void ValidateEventTurn(Player? player, EventCard? eventCard)
        {
            if (player == null)
            {
                throw new InvalidDataException("驗證事件卡失敗：找不到人物");
            }
            if (eventCard == null)
            {
                throw new InvalidDataException("驗證事件卡失敗：事件不可為空");
            }
            if (string.IsNullOrEmpty(eventCard.Title) || eventCard.Amount == 0)
            {
                throw new InvalidDataException("驗證事件卡失敗：無效的事件設定");
            }
        }
    }
}
