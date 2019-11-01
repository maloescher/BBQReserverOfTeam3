using System;
using System.Linq;
using BBQReserverBot.Model.Entities;
using Telegram.Bot.Types;

namespace BBQReserverBot.Model
{
    public class RecordModel
    {
        public static Record createRecord(User user, int selectedDay, int selectedMonth, int selectedStart,
            int selectedEnd)
        {
            var record = new Record(user, selectedDay, selectedMonth, selectedStart, selectedEnd);
            return record;
        }

        public static bool createRecord(User user, int selectedDay, int selectedMonth, int selectedStart,
            int selectedEnd, bool checkAndWrite)
        {
            if (!checkAndWrite) return false;
            try
            {
                var record = createRecord(user, selectedDay, selectedMonth, selectedStart, selectedEnd);
                if (CheckForTimeIntersections(record)) throw new ArgumentException("Date already taken");

                Schedule.Records.Add(record);
                return true;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool deleteRecord(Record record)
        {
            return Schedule.Records.Contains(record) && Schedule.Records.Remove(record);
        }

        public static bool updateRecord(Record oldRecord, User user, int newStart, int newEnd)
        {
            if (user.Id != oldRecord.User.Id || !Schedule.Records.Contains(oldRecord)) return false;
            var day = oldRecord.FromTime.Day;
            var month = oldRecord.FromTime.Month;
            try
            {
                var record = createRecord(user, day, month, newStart, newEnd);
                if (CheckForTimeIntersectionsExcept(record, oldRecord))
                    throw new ArgumentException("Change not possible, overlapping");
                deleteRecord(oldRecord);
                Schedule.Records.Add(record);
                return true;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private static bool CheckForTimeIntersections(Record newRecord)
        {
            var intersections = from records in Schedule.Records
                where records.FromTime < newRecord.ToTime && records.ToTime > newRecord.FromTime
                select records;
            return intersections.Any();
        }

        private static bool CheckForTimeIntersectionsExcept(Record newRecord, Record exceptRecord)
        {
            var intersections = from records in Schedule.Records
                where records.FromTime < newRecord.ToTime && records.ToTime > newRecord.FromTime 
                select records;
            intersections = from records in intersections
                where records.Id != exceptRecord.Id
                select records;
            return intersections.Any();
        }
    }
}