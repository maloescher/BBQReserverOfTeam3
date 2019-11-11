using System;
using System.Collections.Generic;
using System.Linq;
using BBQReserverBot.Model.Entities;
using Telegram.Bot.Types;

namespace BBQReserverBot.Model
{
    public class RecordModel
    {
        public static Record CreateRecord(User user, int selectedDay, int selectedMonth, int selectedStart,
            int selectedEnd)
        {
            var record = new Record(user, selectedDay, selectedMonth, selectedStart, selectedEnd);
            return record;
        }

        public static bool CreateRecord(User user, int selectedDay, int selectedMonth, int selectedStart,
            int selectedEnd, bool checkAndWrite)
        {
            if (!checkAndWrite) return false;
            try
            {
                var record = CreateRecord(user, selectedDay, selectedMonth, selectedStart, selectedEnd);
                if (CheckForTimeIntersections(record)) throw new ArgumentException("Date already taken");
                if (RecordIsInPast(record)) throw new ArgumentException("Date lies in past");

                RecordModel.GetAllRecords().Add(record);
                return true;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool DeleteRecord(Record record)
        {
            return RecordModel.GetAllRecords().Contains(record) && RecordModel.GetAllRecords().Remove(record);
        }

        public static bool UpdateRecord(Record oldRecord, User user, int newStart, int newEnd)
        {
            if (user.Id != oldRecord.User.Id || !RecordModel.GetAllRecords().Contains(oldRecord)) return false;
            var day = oldRecord.FromTime.Day;
            var month = oldRecord.FromTime.Month;
            try
            {
                var record = CreateRecord(user, day, month, newStart, newEnd);
                if (CheckForTimeIntersectionsExcept(record, oldRecord))
                    throw new ArgumentException("Change not possible, overlapping");
                DeleteRecord(oldRecord);
                RecordModel.GetAllRecords().Add(record);
                return true;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static Record FindRecordByUserInputString(string text)
        {
            foreach (var record in RecordModel.GetAllRecords())
            {
                if (text.Equals(record.FromTime.ToString("dd MMMM, HH:mm") + "—" + record.ToTime.Hour + ":00"))
                    return record;
            }

            return null;
        }

        public static List<Record> GetAllRecords()
        {
            return Schedule.Records;
        }

        private static bool CheckForTimeIntersections(Record newRecord)
        {
            var intersections = from records in RecordModel.GetAllRecords()
                where records.FromTime < newRecord.ToTime && records.ToTime > newRecord.FromTime
                select records;
            return intersections.Any();
        }

        private static bool CheckForTimeIntersectionsExcept(Record newRecord, Record exceptRecord)
        {
            var intersections = from records in RecordModel.GetAllRecords()
                where records.FromTime < newRecord.ToTime && records.ToTime > newRecord.FromTime
                select records;
            intersections = from records in intersections
                where records.Id != exceptRecord.Id
                select records;
            return intersections.Any();
        }

        private static bool RecordIsInPast(Record record)
        {
            var now = DateTime.Now;
            //record is before now, return true
            return DateTime.Compare(now, record.FromTime) >= 0;
        }
    }
}