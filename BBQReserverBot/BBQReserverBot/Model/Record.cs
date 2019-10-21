using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBQReserverBot.Model
{
    public class Record
    {
        public Guid Id { get; set; }
        public User User { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public string Comment { get; set; }

    }
}
