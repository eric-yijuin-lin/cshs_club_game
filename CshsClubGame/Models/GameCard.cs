namespace CshsClubGame.Models
{
    public class CardHelper
    {
        private static readonly CardType[] _turnProbabilities =
        {
            CardType.Character,
            CardType.Character,
            CardType.Character,
            CardType.Character,
            CardType.Character,
            CardType.Character,
            CardType.Character, // 角色 (戰鬥) 卡 70%
            CardType.Npc, // NPC (戰鬥) 卡 10%
            CardType.Equipment, // 裝備卡 10%
            CardType.Event, // 事件 (休息) 卡 10%
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
                return CharaterCard.NewNpcCard();
            }

            int index = rand.Next(rand.Next(0, oppoentCandidates.Count));
            var opponent = oppoentCandidates[index];
            var card = new CharaterCard()
            {
                Id = opponent.Id,
                Atk = opponent.Atk,
                CardType = CardType.Character,
                Description = $"一個來自 {opponent.ClassUnit} 的冒險者",
                Equipments = opponent
                            .EquipmentList
                            .Select(x => x.Name).ToList(),
                Level = opponent.Level,
                Quality = ItemQuality.Normal,
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
                CardType = CardType.Equipment,
                Description = equip.Description,
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
                CardType = CardType.Event,
                Description = "研究顯示，睡覺有益身心健康",
                Amount = 20,
                EventId = "休息",
                Quality = ItemQuality.Normal,
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
                CardType cardType = _turnProbabilities[index];
                Console.WriteLine($"cardType {i}：{cardType}"); 
                switch (cardType)
                {
                    case CardType.Character:
                        var opponent = this.GetOpponentCard(playerId, candidates);
                        cards.Add(opponent);
                        break;
                    case CardType.Npc: // NPC should be refactor to Character Type later
                        var npc = CharaterCard.NewNpcCard();
                        cards.Add(npc);
                        break;
                    case CardType.Equipment:
                        var equipment = this.GetEquipmentCard();
                        cards.Add(equipment);
                        break;
                    case CardType.Event:
                        var eventCard = this.GetEventCard();
                        cards.Add(eventCard);
                        break;
                    default:
                        throw new InvalidDataException("無效的卡片類型");
                }
            }
            Console.WriteLine("return cards; " + cards.Count);
            return cards;
        }
    }
    public abstract class GameCard
    {
        public CardType CardType { get; set; } = CardType.Undefined;
        public ItemQuality Quality { get; set; }
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

        public static CharaterCard NewNpcCard(int level = 0)
        {
            if (level == 0)
            {
                int rand = new Random().Next(10);
                if (rand < 6)
                {
                    level = 1;
                }
                else if (rand < 8)
                {
                    level = 2;
                }
                else
                {
                    level = 3;
                }
            }
            var npc = new CharaterCard()
            {
                Id = "npc",
                CardType = CardType.Character,
                Level = level,
                Hp = 5,
                Rank = 0,
                Atk = 1,
                Description = "123 木頭人",
                Quality = ItemQuality.Normal,
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
                    npc.Quality = ItemQuality.Advanced;
                    npc.Title = "菁英木人";
                    return npc;
                case 3:
                    npc.Level = 3;
                    npc.Atk = 3;
                    npc.Description = "789 木頭人";
                    npc.Quality = ItemQuality.Epic;
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

    public enum CardType
    {
        Undefined,
        Character,
        Npc,
        Equipment,
        Event
    }

    public enum ItemQuality
    {
        Normal,
        Advanced,
        Epic
    }
}
