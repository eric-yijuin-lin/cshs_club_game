using System.Collections.Concurrent;

namespace CshsClubGame.Models
{
    public class GameHistoryHelper
    {
        private static long _serialNo = 0;
        private readonly ConcurrentDictionary<DateTime, ConcurrentBag<GameHistoryEntry>> _history;
        private readonly ConcurrentDictionary<DateTime, long> _maxPageSerialNo;

        public GameHistoryHelper()
        {
            _history = new ConcurrentDictionary<DateTime, ConcurrentBag<GameHistoryEntry>>();
            _maxPageSerialNo = new ConcurrentDictionary<DateTime, long>();
        }

        public GameHistoryEntry[] GetHistoryPage(DateTime dateTime)
        {
            var timeMark = GameHistoryHelper.RoundUp(dateTime, TimeSpan.FromMinutes(1));
            if (!_history.TryGetValue(timeMark, out ConcurrentBag<GameHistoryEntry>? historyPage))
            {
                return new GameHistoryEntry[0];
            }
            return historyPage.ToArray();
        }

        public void AddJoinRoomHistory(Player? player)
        {
            var entry = GameHistoryEntry.CreateJoinRoomHistoryEntry(player);
            this.AddHistory(entry);
        }
        public void AddBattleHistory(Player me, Player target, BattleRecord record)
        {
            var entry = GameHistoryEntry.CreateBattleHistoryEntry(me, target, record);
            this.AddHistory(entry);
        }
        public void AddEquipHistory(Player me, Equipment equipment)
        {
            var entry = GameHistoryEntry.CreateEquipHistoryEntry(me, equipment);
            this.AddHistory(entry);
        }
        public void AddEventHistory(Player me, EventCard eventCard)
        {
            var entry = GameHistoryEntry.CreateEventHistoryEntry(me, eventCard);
            this.AddHistory(entry);
        }
        public void ClearHistory()
        {
            _serialNo = 0;
            _history.Clear();
        }

        private void AddHistory(GameHistoryEntry entry)
        {
            entry.EntryDateTime = DateTime.Now;
            var timeMark = GameHistoryHelper.RoundUp(entry.EntryDateTime, TimeSpan.FromMinutes(1));
            entry.SerialNo = ++GameHistoryHelper._serialNo;

            if (_history.ContainsKey(timeMark))
            {
                _history[timeMark].Add(entry);
            }
            else
            {
                _history[timeMark] = new ConcurrentBag<GameHistoryEntry>() { entry };
            }
        }

        private static DateTime RoundUp(DateTime dt, TimeSpan span)
        {
            return new DateTime((dt.Ticks + span.Ticks - 1) / span.Ticks * span.Ticks, dt.Kind);
        }
    }

    public class GameHistoryEntry
    {
        public DateTime EntryDateTime { get; set; }
        public long SerialNo { get; set; }
        public string ClassUnit { get; set; } = null!;
        public string PlayerName { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string DateTimeString { get { return EntryDateTime.ToString("HH:mm:ss"); } }

        public GameHistoryEntry(Player player, string message)
        {
            ClassUnit = player.ClassUnit;
            PlayerName = player.Name;
            Message = message;
        }

        public static GameHistoryEntry CreateJoinRoomHistoryEntry(Player? player)
        {
            if (player == null)
            {
                throw new InvalidDataException("建立日誌失敗，玩家不可為空");
            }

            string message = $"{player.ClassUnit} 的 {player.Name} 進到房間，狩獵開始了，嘿嘿嘿嘿！";
            return new GameHistoryEntry(player, message);
        }

        public static GameHistoryEntry CreateBattleHistoryEntry(Player me, Player target, BattleRecord battleRecord)
        {
            string message = $"{me.ClassUnit} 的 {me.Name} 對 {target.ClassUnit} 的 {target.Name} 發動攻擊！";
            if (me.Hp > 0 && target.Hp <= 0)
            {
                message += "並且打倒了他！";
                if (battleRecord.LootExpEquipment != null)
                {
                    message += $"還從他身上奪走了 {battleRecord.LootExpEquipment.Name}！";
                }
            }
            else if (me.Hp > 0 && target.Hp > 0)
            {
                message += "雙方互有往來，打到天黑各自回家吃飯。";
            }
            else if (me.Hp <= 0 && target.Hp > 0)
            {
                message += "結果自己掛了QQ";
            }
            else
            {
                message += "然後兩個人同歸於盡了QQ";
            }
            return new GameHistoryEntry(me, message);
        }

        public static GameHistoryEntry CreateEquipHistoryEntry(Player player, Equipment equipment)
        {
            string message = $"{player.ClassUnit} 的 {player.Name} 撿到珍貴的 {equipment.Name}，戰力大幅提升！";
            return new GameHistoryEntry(player, message);
        }

        public static GameHistoryEntry CreateEventHistoryEntry(Player player, EventCard eventCard)
        {
            string message = $"{player.ClassUnit} 的 {player.Name} 覺得上課很無聊，趴在桌上呼呼大睡，恢復了 {eventCard.Amount} 點生命力！";
            return new GameHistoryEntry(player, message);
        }
    }

    public class TurnRecord
    {
        public TurnStatus Status { get; set; } = TurnStatus.NotProceeded;
        public TurnType TurnType { get; set; }
        public string? Message { get; set; }
        public Player? PlayerMe { get; set; }
        public BattleRecord? BattleRecord { get; set; }

        public TurnRecord(TurnType type)
        {
            this.TurnType = type;
        }
    }

    public class BattleRecord
    {
        public int SelfHp { get; set; }
        public int TargetHp { get; set; }
        public int LootExp { get; set; }
        public int LootRankScore { get; set; }
        public Equipment? LootExpEquipment { get; set; }
        public DateTime BattleTime { get; set; }
    }

    public enum TurnStatus
    {
        NotProceeded,
        Ok,
        Error
    }
}
