using System;
using System.Linq;
using Telegram.Bot.Types;
namespace BBQReserverBot.Model.Entities
{
    public class Record
    {
        public Record()
        {
        }

        public Record(User user, int selectedDay, int selectedMonth, int selectedStart, int selectedEnd)
        {
            Id = Guid.NewGuid();
            User = user;
            FromTime = new DateTime(DateTime.Now.Year, selectedMonth, selectedDay, selectedStart, 0, 0);
            ToTime = new DateTime(DateTime.Now.Year, selectedMonth, selectedDay, selectedEnd, 0, 0);
            if (FromTime < ToTime)
                ToTime = ToTime.AddDays(1);

            if (FromTime < DateTime.Now)
            {
                FromTime = FromTime.AddYears(1);
                ToTime = ToTime.AddYears(1);
            }
        }

        public Guid Id { get; set; }
        public User User { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public string Comment { get; set; }
    }
}