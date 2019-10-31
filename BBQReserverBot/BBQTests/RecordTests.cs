using System;
using NUnit.Framework;
using BBQReserverBot;
using BBQReserverBot.Dialogues;
using BBQReserverBot.Model;
using Telegram.Bot.Types;

namespace BBQTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var record = new Record
            {
                Id = Guid.NewGuid(),
                FromTime = new DateTime(DateTime.Now.Year, 12, 02, 18, 0, 0),
                ToTime = new DateTime(DateTime.Now.Year, 12, 02, 22, 0, 0)
            };
            AddRecord(record);
        }
        
        [Test]
        public void CreateRecord()
        {
            var user = new User();
            var size = Schedule.Records.Count;
            CreateRecordWithDialogueClass(user, "1", "September", "19:00", "22:00");
            
            Assert.AreEqual(size + 1, Schedule.Records.Count);
            StringAssert.AreEqualIgnoringCase("2019-09-01 19:00:00",
                Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        
        [Test]
        public void CreateOverlappingRecord()
        {
            var user = new User();
            var size = Schedule.Records.Count;
            CreateRecordWithDialogueClass(user, "2", "September", "19:00", "22:00");
            
            Assert.AreEqual(size + 1, Schedule.Records.Count);
            StringAssert.AreEqualIgnoringCase("2019-09-02 19:00:00",
                Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));

            var user2 = new User();
            user2.Id = 2;
            
            Assert.AreEqual(size + 1, Schedule.Records.Count); // Not +2 as we expect not to write into Schedule
            StringAssert.AreEqualIgnoringCase("2019-09-02 19:00:00",
                Schedule.Records[0].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        
        [Test]
        public void UpdateRecord()
        {
            var user = new User();
            var size = Schedule.Records.Count;
            CreateRecordWithDialogueClass(user, "1", "August", "19:00", "22:00");
            
            Assert.AreEqual(size + 1, Schedule.Records.Count);
            StringAssert.AreEqualIgnoringCase("2019-08-01 19:00:00",
                Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));
            
            CreateRecordWithDialogueClass(user, "1", "August", "18:00", "22:00");
            Assert.AreEqual(size + 1, Schedule.Records.Count);
            StringAssert.AreEqualIgnoringCase("2019-08-01 18:00:00", Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        
        private void AddRecord(Record record)
        {
            var size = Schedule.Records.Count;
            Schedule.Records.Add(record);
            Assert.AreEqual(size + 1, Schedule.Records.Count);
        }

        private void CreateRecordWithDialogueClass(User user, String day, String month, String startTime, String endTime)
        {
            var recordCreator = new CreateRecordDialogue(null);
            recordCreator.ProcessMonth(month);
            recordCreator.ProcessDay(day);
            recordCreator.ProcessTime(startTime, true);
            recordCreator.ProcessTime(endTime, false);
            recordCreator.ProcessApprove("Approve");
            recordCreator.Create(user);
        }
    }
}