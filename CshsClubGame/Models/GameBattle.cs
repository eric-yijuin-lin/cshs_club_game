namespace CshsClubGame.Models
{
    public class BattleRecord
    {
        public int SelfHp { get; set; }
        public int TargetHp { get; set; }
        public int LootExp { get; set; }
        public Equipment? LootExpEquipment { get; set; }
        public DateTime BattleTime { get; set; }
    }
}
