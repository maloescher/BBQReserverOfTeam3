using System;
using System.Linq;
using Telegram.Bot.Types;

namespace BBQReserverBot.Model
{
    public class Record
    {
        
        public Record() {}
        public Record(User user, int selectedDay, int selectedMonth, int selectedStart, int selectedEnd) 
        {
            Id = Guid.NewGuid();
            User = user;
            FromTime = new DateTime(DateTime.Now.Year, selectedMonth, selectedDay, selectedStart, 0, 0);
            ToTime = new DateTime(DateTime.Now.Year, selectedMonth, selectedDay, selectedEnd, 0, 0);
            if (FromTime < ToTime)
                ToTime.AddDays(1);
            
            if (FromTime < DateTime.Now)
            {
                FromTime.AddYears(1);
                ToTime.AddYears(1);
            }
            if (checkForTimeIntersections(this))
                throw new ArgumentException("Date not possible"); 
        }
        public Guid Id { get; set; }
        public User User { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public string Comment { get; set; }
        
        private bool checkForTimeIntersections(BBQReserverBot.Model.Record record)
        {
            var intersections = from r in Schedule.Records
                where r.FromTime < record.ToTime && r.ToTime > record.FromTime
                select r;
            return intersections.Any();
        }
    }
}
