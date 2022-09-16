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
        public bool HasEquipment { get { return EquipmentList.Any(); } }
        public List<Equipment> EquipmentList = new List<Equipment>();

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
    }
}
