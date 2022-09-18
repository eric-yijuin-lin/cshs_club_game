namespace CshsClubGame.Models
{
    public class LootHelper
    {
        private static double _dropEquipProbability = 0.5;
        private static Equipment[] _dropEquips =
        {
            new Equipment() { Quality = "普通", Name = "衛生紙", EnhancedAtk = 0, EnhancedHp = 1, Description = "皺巴巴的衛生紙掛在身上別人就會對攻擊你有所遲疑" },
            new Equipment() { Quality = "普通", Name = "衛生紙", EnhancedAtk = 0, EnhancedHp = 1, Description = "皺巴巴的衛生紙掛在身上別人就會對攻擊你有所遲疑"  },
            new Equipment() { Quality = "普通", Name = "衛生紙", EnhancedAtk = 0, EnhancedHp = 1, Description = "皺巴巴的衛生紙掛在身上別人就會對攻擊你有所遲疑" },
            new Equipment() { Quality = "普通", Name = "橡皮筋", EnhancedAtk = 1, EnhancedHp = 0, Description = "即使不會受傷，敵人還是會很怕被射到" },
            new Equipment() { Quality = "普通", Name = "橡皮筋", EnhancedAtk = 1, EnhancedHp = 0, Description = "即使不會受傷，敵人還是會很怕被射到" },
            new Equipment() { Quality = "普通", Name = "橡皮筋", EnhancedAtk = 1, EnhancedHp = 0, Description = "即使不會受傷，敵人還是會很怕被射到" },
            new Equipment() { Quality = "普通", Name = "厚紙板", EnhancedAtk = 0, EnhancedHp = 2, Description = "別小看這紙做的板子，股市大跌的時候它可非常搶手" },
            new Equipment() { Quality = "普通", Name = "厚紙板", EnhancedAtk = 0, EnhancedHp = 2, Description = "別小看這紙做的板子，股市大跌的時候它可非常搶手" },
            new Equipment() { Quality = "普通", Name = "紙飛機", EnhancedAtk = 2, EnhancedHp = 0, Description = "這種無人機在美國崛起之前就已經存在千年之久" },
            new Equipment() { Quality = "普通", Name = "紙飛機", EnhancedAtk = 2, EnhancedHp = 0, Description = "這種無人機在美國崛起之前就已經存在千年之久" },
            new Equipment() { Quality = "普通", Name = "洋芋片包裝", EnhancedAtk = 0, EnhancedHp = 3, Description = "此裝備的正式名稱為「氣態氮護具」，裡面的馬鈴薯片是不小心放進去的" },
            new Equipment() { Quality = "普通", Name = "洋芋片包裝", EnhancedAtk = 0, EnhancedHp = 3, Description = "此裝備的正式名稱為「氣態氮護具」，裡面的馬鈴薯片是不小心放進去的" },
            new Equipment() { Quality = "普通", Name = "巧克力棒", EnhancedAtk = 3, EnhancedHp = 0, Description = "不要懷疑巧克力棒是一種武器，它會甜死人" },
            new Equipment() { Quality = "普通", Name = "巧克力棒", EnhancedAtk = 3, EnhancedHp = 0, Description = "不要懷疑巧克力棒是一種武器，它會甜死人" },
            new Equipment() { Quality = "普通", Name = "學校的桌子", EnhancedAtk = 0, EnhancedHp = 10, Description = "竹山高中教室裡堅固耐用的桌子，上面好像有人用立可白告白" },
            new Equipment() { Quality = "普通", Name = "學校的椅子", EnhancedAtk = 10, EnhancedHp = 0, Description = "竹山高中教室裡的椅子，傷痕累累，看似被拿來當投射武器很多次" },
            new Equipment() { Quality = "普通", Name = "木板", EnhancedAtk = 0, EnhancedHp = 15, Description = "家政教室找到的厚實木板，好像是有人忘記帶回家的砧板" },
            new Equipment() { Quality = "普通", Name = "菜刀", EnhancedAtk = 15, EnhancedHp = 0, Description = "家政教室找到的沉重菜刀，聽說家政老師都用它來割韭菜" },
            new Equipment() { Quality = "精良", Name = "教官的制服", EnhancedAtk = 0, EnhancedHp = 20, Description = "墊肩上繡著五個星星，穿上它立刻散發出一股威嚴的氣息" },
            new Equipment() { Quality = "精良", Name = "老師的課本", EnhancedAtk = 20, EnhancedHp = 0, Description = "一本一千頁的硬殼教科書，老師拿在手上總令同學害怕" },
            new Equipment() { Quality = "精良", Name = "騎士鎧甲", EnhancedAtk = 0, EnhancedHp = 25, Description = "學校倉庫翻出來的古老盔甲，不曉得如何穿越道現代的" },
            new Equipment() { Quality = "精良", Name = "法師之杖", EnhancedAtk = 25, EnhancedHp = 0, Description = "據說單身 30 年的人就可以學會火球術，這把魔障應該是 30 歲以上的人所製作" },
            new Equipment() { Quality = "史詩", Name = "美國隊長盾牌", EnhancedAtk = 25, EnhancedHp = 40, Description = "攝影棚偷拿出來的盾牌，沒想到堅固無比，還內建遙控飛行功能" },
            new Equipment() { Quality = "史詩", Name = "索爾的槌子", EnhancedAtk = 40, EnhancedHp = 0, Description = "攝影棚偷出來的槌子，手握上去就會有被電到的感覺" },
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
        public string Quality { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int EnhancedHp { get; set; }
        public int EnhancedAtk { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
