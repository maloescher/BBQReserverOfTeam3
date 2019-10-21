using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBQReserverBot.Model
{
    public class Schedule
    {
        public static bool CreateRecord(Record record)
        {
            Records.Add(record);
            return true;
        }
        public static List<Record> Records { get; set; }
    }
}
