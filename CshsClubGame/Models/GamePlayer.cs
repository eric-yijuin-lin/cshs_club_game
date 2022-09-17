namespace CshsClubGame.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string RoomId { get; set; } = null!;
        public string ClassUnit { get; set; }
        public string SeatNo { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Hp { get; set; }
        public int Atk { get; set; }
        public int Rank { get; set; }
        public int SurvivedDay { get; set; }
        public bool HasEquipment { get { return EquipmentList.Any(); } }
        public List<Equipment> EquipmentList = new List<Equipment>();

        private int Exp { get; set; }

        public Player(string classUnit, string seatNo, string name)
        {
            Id = Guid.NewGuid().ToString();
            ClassUnit = classUnit;
            SeatNo = seatNo;
            Name = name;
            Level = 1;
            Hp = 10;
            Atk = 5;
            Rank = 0;
            this.EquipmentList = new List<Equipment>();
        }

        public void AddEquipment(Equipment? equipment)
        {
            if (equipment == null)
            {
                return;
            }
            this.EquipmentList.Add(equipment);
            this.Atk += equipment.EnhancedAtk;
            this.Hp += equipment.EnhancedHp;
            this.SurvivedDay += 1;
        }

        public void AddExp(int exp)
        {
            this.Exp += exp;
            this.ConsumeExp();
        }

        public void Rest(EventCard eventCard)
        {
            this.Hp += eventCard.Amount;
        }

        private void ConsumeExp()
        {
            while (this.Exp >= LevelInfo.ExpMap[this.Level].LevelUpExp)
            {
                this.Exp -= LevelInfo.ExpMap[this.Level].LevelUpExp;
                this.Level += 1;
                this.Atk = StrengthInfo.StrengthMap[this.Level].Atk;
                this.Hp = StrengthInfo.StrengthMap[this.Level].Hp;
            }
        }
    }
    public class LevelInfo
    {
        public static readonly LevelInfo[] ExpMap = new LevelInfo[]
        {
            new LevelInfo() { LevelUpExp = 0, LootExp = 0},
            new LevelInfo() { LevelUpExp = 30, LootExp = 50},
            new LevelInfo() { LevelUpExp = 70, LootExp = 50},
            new LevelInfo() { LevelUpExp = 120, LootExp = 70},
            new LevelInfo() { LevelUpExp = 170, LootExp = 80},
            new LevelInfo() { LevelUpExp = 220, LootExp = 100},
            new LevelInfo() { LevelUpExp = 300, LootExp = 120},
            new LevelInfo() { LevelUpExp = 400, LootExp = 150},
            new LevelInfo() { LevelUpExp = 500, LootExp = 200},
            new LevelInfo() { LevelUpExp = 650, LootExp = 250},
            new LevelInfo() { LevelUpExp = 0, LootExp = 400},
            new LevelInfo() { LevelUpExp = int.MaxValue, LootExp = int.MaxValue},
        };
        public int LevelUpExp { get; set; }
        public int LootExp { get; set; }
    }

    public class StrengthInfo
    {
        public static readonly StrengthInfo[] StrengthMap = new StrengthInfo[]
        {
            new StrengthInfo() { Atk = 5, Hp = 10},
            new StrengthInfo() { Atk = 7, Hp = 15},
            new StrengthInfo() { Atk = 10, Hp = 20},
            new StrengthInfo() { Atk = 15, Hp = 25},
            new StrengthInfo() { Atk = 20, Hp = 35},
            new StrengthInfo() { Atk = 25, Hp = 50},
            new StrengthInfo() { Atk = 35, Hp = 65},
            new StrengthInfo() { Atk = 50, Hp = 85},
            new StrengthInfo() { Atk = 75, Hp = 150},
            new StrengthInfo() { Atk = 100, Hp = 300},
        };
        public int Atk { get; set; }
        public int Hp { get; set; }
    }
}
