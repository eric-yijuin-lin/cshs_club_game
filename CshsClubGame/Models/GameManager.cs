﻿using System.Text.Json;
using System.Text.Json.Nodes;

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

        public TurnRecord? ProcessTurnCard(string selfId, JsonObject card)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            string? cardType = card["cardType"]?.GetValue<string>();
            switch (cardType)
            {
                case "角色":
                    var charaterCard = card.Deserialize<CharaterCard>(options);
                    return this.ProcessBattleCard(selfId, charaterCard);
                case "裝備":
                    var equipmentCard = card.Deserialize<EquipmentCard>(options);
                    return this.ProcessEquipmentCard(selfId, equipmentCard);
                case "事件":
                    var eventCard = card.Deserialize<EventCard>(options);
                    return this.ProcessEventCard(selfId, eventCard);
                default:
                    return null;
            }
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

        private TurnRecord ProcessBattleCard(string selfId, CharaterCard? charaterCard)
        {
            if (charaterCard == null || string.IsNullOrEmpty(charaterCard.Id))
            {
                throw new InvalidDataException("對手 ID 不可為空");
            }
            string? roomId = this.GetRoomIdByPlayerId(selfId);
            if (string.IsNullOrEmpty(roomId))
            {
                throw new InvalidDataException("找不到玩家的房間");
            }

            var self = this.GetPlayer(selfId);
            var target = this.GetPlayer(charaterCard.Id);
            string validateResult = this.ValidateBattle(roomId, self, target);
            if (validateResult != "OK")
            {
                throw new InvalidDataException(validateResult);
            }

            var battleRecord = this.ProcessBattle(self, target);
            self.SurvivedDay += 1;
            return new TurnRecord()
            {
                BattleRecord = battleRecord,
                SelfStatus = self,
                TurnType = "角色"
            };
        }

        private string ValidateBattle(string roomId, Player? player, Player? target)
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

        private BattleRecord ProcessBattle(Player player, Player target)
        {
            player.Hp -= target.Atk;
            player.SurvivedDay += 1;
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
                int exp = _lootHelper.GetLootExp(player, target);
                var equipment = _lootHelper.GetLootEquipment(target);
                player.AddExp(exp);
                player.AddEquipment(equipment);
                battleResult.LootExp = exp;
                battleResult.LootExpEquipment = equipment;
                this.DeletePlayer(target);
            }
            return battleResult;
        }

        private TurnRecord ProcessEquipmentCard(string selfId, EquipmentCard? equipmentCard)
        {
            var player = this.GetPlayer(selfId);
            this.ValidateEquipmentTurn(player, equipmentCard);

            var equip = equipmentCard!.ConvertToEquipment();
            player!.AddEquipment(equip);
            player!.SurvivedDay += 1;
            return new TurnRecord()
            {
                BattleRecord = null,
                SelfStatus = player,
                TurnType = "裝備"
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
                || equipmentCard.EnhancedHp == 0 
                || equipmentCard.EnhancedAtk == 0)
            {
                throw new InvalidDataException("驗證裝備卡失敗：無效的裝備屬性");
            }
        }

        private TurnRecord ProcessEventCard(string selfId, EventCard? eventCard)
        {
            var player = this.GetPlayer(selfId);
            this.ValidateEventTurn(player, eventCard);
            player!.Rest(eventCard!);
            player!.SurvivedDay += 1;
            return new TurnRecord()
            {
                BattleRecord = null,
                SelfStatus = player,
                TurnType = "事件"
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
