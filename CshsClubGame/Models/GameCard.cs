﻿namespace CshsClubGame.Models
{
    public class CardHelper
    {
        private static readonly string[] _turnProbabilities =
        {
            "角色",
            "角色",
            "角色",
            "角色",
            "角色",
            "角色",
            "角色", // 角色 (戰鬥) 卡 70%
            "NPC", // NPC (戰鬥) 卡 10%
            "裝備", // 裝備卡 10%
            "事件", // 事件 (休息) 卡 10%
        };

        private List<Player> GetOpponentCandidates(string playerId, Dictionary<string, Player> allPlayers)
        {
            var rand = new Random();
            var candidates = new List<Player>();
            var otherPlayers = allPlayers.Values
                .Where(x => x.Id != playerId)
                .ToList();
            while (candidates.Count < 5 && otherPlayers.Count > 0 )
            {
                int index = rand.Next(0, otherPlayers.Count);
                candidates.Add(otherPlayers[index]);
                otherPlayers.RemoveAt(index);
            }
            return candidates;
        }

        private CharaterCard GetOpponentCard(string playerId, List<Player> oppoentCandidates)
        {
            var rand = new Random();
            if (oppoentCandidates.Count == 0)
            {
                int npcLevel = rand.Next(1, 4);
                return CharaterCard.NewNpcCard(npcLevel);
            }

            int index = rand.Next(rand.Next(0, oppoentCandidates.Count));
            var opponent = oppoentCandidates[index];
            var card = new CharaterCard()
            {
                Id = opponent.Id,
                Atk = opponent.Atk,
                CardType = "角色",
                Description = $"一個來自 {opponent.ClassUnit} 的冒險者",
                Equipments = opponent
                            .EquipmentList
                            .Select(x => x.Name).ToList(),
                Level = opponent.Level,
                Quality = "史詩",
                Rank = opponent.Rank,
                Title = opponent.Name,
                Hp = opponent.Hp
            };
            oppoentCandidates.RemoveAt(index); // 暫時寫有 side effect 的髒扣，有空再改
            return card;
        }

        private EquipmentCard GetEquipmentCard()
        {
            var equip = LootHelper.GetRandomEquipment();
            return new EquipmentCard()
            {
                CardType = "裝備",
                Description = "一個稀世的珍寶",
                EnhancedAtk = equip.EnhancedAtk,
                EnhancedHp = equip.EnhancedHp,
                Quality = equip.Quality,
                Title = equip.Name
            };
        }

        private EventCard GetEventCard()
        {
            return new EventCard()
            {
                CardType = "事件",
                Description = "研究顯示，睡覺有益身心健康",
                Amount = 20,
                EventId = "休息",
                Quality = "普通",
                Title = "休息"
            };
        }

        public List<GameCard> GetTurnCards(string playerId, Dictionary<string, Player> allPlayers)
        {
            var rand = new Random();
            var cards = new List<GameCard>();
            var candidates = GetOpponentCandidates(playerId, allPlayers);
            for (int i = 0; i < 5; i++)
            {
                int index = rand.Next(0, _turnProbabilities.Length);
                string cardType = _turnProbabilities[index];

                switch (cardType)
                {
                    case "角色":
                        var opponent = this.GetOpponentCard(playerId, candidates);
                        cards.Add(opponent);
                        break;
                    case "NPC":
                        int npcLevel = rand.Next(4);
                        var npc = CharaterCard.NewNpcCard(npcLevel);
                        cards.Add(npc);
                        break;
                    case "裝備":
                        var equipment = this.GetEquipmentCard();
                        cards.Add(equipment);
                        break;
                    case "事件":
                        var eventCard = this.GetEventCard();
                        cards.Add(eventCard);
                        break;
                    default:
                        throw new InvalidDataException("無效的卡片類型");
                }
            }
            return cards;
        }
    }
    public abstract class GameCard
    {
        public string CardType { get; set; } = null!;
        public string Quality { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    public class EquipmentCard : GameCard
    {
        public int EnhancedAtk { get; set; }
        public int EnhancedHp { get; set; }

        public Equipment ConvertToEquipment()
        {
            return new Equipment()
            {
                EnhancedAtk = EnhancedAtk,
                EnhancedHp = EnhancedHp,
                Name = this.Title,
                Quality = this.Quality
            };
        }
    }

    public class CharaterCard : GameCard
    {
        public string Id { get; set; } = null!;
        public int Level { get; set; }
        public int Rank { get; set; }
        public List<string> Equipments { get; set; } = new List<string>();
        public int Atk { get; set; }
        public int Hp { get; set; }

        public static CharaterCard NewNpcCard(int level)
        {
            var npc = new CharaterCard()
            {
                Id = "npc",
                CardType = "角色",
                Level = level,
                Hp = 5,
                Rank = 0,
                Atk = 1,
                Description = "123 木頭人",
                Quality = "普通",
                Title = "訓練場木人"
            };
            switch (level)
            {
                case 1:
                    return npc;
                case 2:
                    npc.Level = 2;
                    npc.Atk = 2;
                    npc.Description = "456 木頭人";
                    npc.Quality = "精良";
                    npc.Title = "菁英木人";
                    return npc;
                case 3:
                    npc.Level = 3;
                    npc.Atk = 3;
                    npc.Description = "789 木頭人";
                    npc.Quality = "史詩";
                    npc.Title = "傳說木人";
                    return npc;
                default:
                    return npc;
            }
        }
    }
    public class EventCard : GameCard
    {
        public string EventId { get; set; } = null!;
        public int Amount { get; set; }
    }
}
