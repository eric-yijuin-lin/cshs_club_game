namespace CshsClubGame.Models
{
    public class TurnRecord
    {
        public string TurnType { get; set; } = null!;
        public Player SelfStatus { get; set; } = null!;
        public BattleRecord? BattleRecord { get; set; }
    }

    public class BattleRecord
    {
        public int SelfHp { get; set; }
        public int TargetHp { get; set; }
        public int LootExp { get; set; }
        public Equipment? LootExpEquipment { get; set; }
        public DateTime BattleTime { get; set; }
    }
}
