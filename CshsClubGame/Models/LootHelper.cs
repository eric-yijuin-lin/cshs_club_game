namespace CshsClubGame.Models
{
    public class LootHelper
    {
        private static double _dropEquipProbability = 0.5;
        private static Equipment[] _dropEquips =
        {
            new Equipment() { Quality = "普通", Name = "衛生紙", EnhancedAtk = 0, EnhancedHp = 1},
            new Equipment() { Quality = "普通", Name = "衛生紙", EnhancedAtk = 0, EnhancedHp = 1},
            new Equipment() { Quality = "普通", Name = "衛生紙", EnhancedAtk = 0, EnhancedHp = 1},
            new Equipment() { Quality = "普通", Name = "橡皮筋", EnhancedAtk = 1, EnhancedHp = 0},
            new Equipment() { Quality = "普通", Name = "橡皮筋", EnhancedAtk = 1, EnhancedHp = 0},
            new Equipment() { Quality = "普通", Name = "橡皮筋", EnhancedAtk = 1, EnhancedHp = 0},
            new Equipment() { Quality = "普通", Name = "厚紙板", EnhancedAtk = 0, EnhancedHp = 2},
            new Equipment() { Quality = "普通", Name = "厚紙板", EnhancedAtk = 0, EnhancedHp = 2},
            new Equipment() { Quality = "普通", Name = "紙飛機", EnhancedAtk = 2, EnhancedHp = 0},
            new Equipment() { Quality = "普通", Name = "紙飛機", EnhancedAtk = 2, EnhancedHp = 0},
            new Equipment() { Quality = "普通", Name = "洋芋片包裝", EnhancedAtk = 0, EnhancedHp = 3},
            new Equipment() { Quality = "普通", Name = "巧克力棒", EnhancedAtk = 3, EnhancedHp = 0},
            new Equipment() { Quality = "普通", Name = "學校的桌子", EnhancedAtk = 0, EnhancedHp = 10},
            new Equipment() { Quality = "普通", Name = "學校的椅子", EnhancedAtk = 10, EnhancedHp = 0},
            new Equipment() { Quality = "普通", Name = "木板", EnhancedAtk = 0, EnhancedHp = 15},
            new Equipment() { Quality = "普通", Name = "菜刀", EnhancedAtk = 15, EnhancedHp = 0},
            new Equipment() { Quality = "精良", Name = "教官的制服", EnhancedAtk = 0, EnhancedHp = 20},
            new Equipment() { Quality = "精良", Name = "老師的課本", EnhancedAtk = 20, EnhancedHp = 0},
            new Equipment() { Quality = "精良", Name = "騎士鎧甲", EnhancedAtk = 0, EnhancedHp = 25},
            new Equipment() { Quality = "精良", Name = "法師之杖", EnhancedAtk = 25, EnhancedHp = 0},
            new Equipment() { Quality = "史詩", Name = "美國隊長盾牌", EnhancedAtk = 25, EnhancedHp = 40},
            new Equipment() { Quality = "史詩", Name = "索爾的槌子", EnhancedAtk = 40, EnhancedHp = 0},
        };

        public static Equipment GetRandomEquipment()
        {
            var random = new Random();
            int index = random.Next(0, _dropEquips.Length);
            return _dropEquips[index];
        }

        private readonly Random _rand = new Random();

        private bool IsEquipmentDropped()
        {
            return _rand.NextDouble() < _dropEquipProbability;
        }

        private Equipment GetEquipmentFromPlayer(Player player)
        {
            int index = _rand.Next(0, player.EquipmentList.Count);
            return player.EquipmentList[index];
        }


        public int GetLootExp(Player player, Player target)
        {
            var levelInfo = LevelInfo.ExpMap[target.Level];
            int characterExp = levelInfo.LootExp;
            int equipExp = target.EquipmentList.Count * 100;
            return characterExp + equipExp;
        }

        public Equipment? GetLootEquipment(Player target)
        {
            if (target.HasEquipment)
            {
                return GetEquipmentFromPlayer(target);
            }
            else
            {
                if (IsEquipmentDropped())
                {
                    GetRandomEquipment();
                }
                return null;
            }
        }

        public int GetLootRankScore(Player? self, Player? target)
        {
            if (self == null || target == null)
            {
                throw new InvalidDataException("計算 rank 分數失敗：角色不可為空");
            }
            if (target.IsNpc)
            {
                return 0;
            }
            int levelDiff = target.Level - self.Level;
            int multiplier = levelDiff < 1 ? 1 : levelDiff * levelDiff;
            return multiplier * target.Level;
        }
    }

    public class Equipment
    {
        public string Quality { get; set; }
        public string Name { get; set; } = string.Empty;
        public int EnhancedHp { get; set; }
        public int EnhancedAtk { get; set; }
    }
}
