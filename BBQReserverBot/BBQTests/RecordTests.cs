using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using BBQReserverBot.Dialogues;
using BBQReserverBot.Model;
using BBQReserverBot.Model.Entities;
using Telegram.Bot.Types;

namespace BBQTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void ClearRecordsAfterEachTest()
        {
            Schedule.Records.Clear();
        }

        /// <summary>
        ///   US002 Tests on UseCase creation of record
        /// </summary>
        static object[] createRecordObject =
        {
            //todo bug found here
            /*new object[] {new DateTime(2019, 03, 20, 8, 0, 0), 13, true, false},
            new object[] {new DateTime(2019, 05, 01, 9, 0, 0), 12, true, false},*/
            new object[] {new DateTime(2019, 04, 01, 10, 0, 0), 12, true , false},
            new object[] {new DateTime(2019, 09, 01, 19, 0, 0), 22, true , false},
            new object[] {new DateTime(2019, 08, 01, 21, 0, 0), 22, true , false},
            new object[] {new DateTime(2019, 07, 01, 22, 0, 0), 22, true , false},
            new object[] {new DateTime(2019, 01, 01, 6, 0, 0), 23, false , true},
            new object[] {new DateTime(2019, 01, 02, 18, 0, 0), 23, false , true},
            new object[] {new DateTime(2019, 01, 03, 6, 0, 0), 22, false , true},
        };
        
        [Test, TestCaseSource("createRecordObject")]
        public void CreateRecord(DateTime startDateTime, int endTimeInt, bool valid, bool outOfRange)
        {
            var user = new User();
            var size = Schedule.Records.Count;
            var day = startDateTime.Day.ToString();
            var month = startDateTime.ToString("MMMM");
            var startTime = startDateTime.ToString("HH:mm");
            var endTime = endTimeInt + ":00";
            
            if (valid)
            {
                CreateRecordWithDialogueClass(user, day, month, startTime, endTime);

                Assert.AreEqual(size + 1, Schedule.Records.Count);
                StringAssert.AreEqualIgnoringCase(startDateTime.ToString("MM-dd HH:mm:ss"),
                    Schedule.Records[size].FromTime.ToString("MM-dd HH:mm:ss"));
            }
            else  {
                if (outOfRange)
                    Assert.Throws<ArgumentOutOfRangeException>(() => CreateRecordWithDialogueClass(user, day, month, startTime, endTime));
                else 
                    Assert.AreEqual(size, Schedule.Records.Count);
            }
        }
        
        //todo probably a bug here
        [Test]
        public void CreateRecordEarlier()
        {
            var user = new User();
            var size = Schedule.Records.Count;
            CreateRecordWithDialogueClass(user, "3", "November", "8:00", "10:00");

            Assert.AreEqual(size + 1, Schedule.Records.Count);
            StringAssert.AreEqualIgnoringCase("2019-11-03 08:00:00",
                Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        
        private static object[] CreateOverlappingRecordObject =
        {
            new object[]
            {
                new DateTime(2019, 09, 20, 10, 0, 0), 12,
                new DateTime(2019, 09, 20, 11, 0, 0), 14
            },
        };
        
        [Test, TestCaseSource("CreateOverlappingRecordObject")]
        public void CreateOverlappingRecord(DateTime startDateTime1, int endTimeInt1, DateTime startDateTime2, int endTimeInt2)
        {
            CreateRecord(startDateTime1, endTimeInt1, true, false);
            CreateRecord(startDateTime2, endTimeInt2, false, false);
        }

        [TestCase(30, 5, false)]
        [TestCase(600, 6, false)]
        [TestCase(65, 4, false)]
        public void CreateNRecords(int count, int month, bool secondRound)
        {
            var user = new User();
            var size = Schedule.Records.Count;
            var counter = 0;
            while (counter < count)
            {
                for (var day = 1; day <= 28; day++)
                {
                    if (counter == count)
                        break;
                    for (var i = 8; i < 21; i++)
                    {
                        if (counter == count)
                            break;
                        if (!secondRound)
                            RecordModel.CreateRecord(user, day, month, i, i + 1, true);
                        //todo bug found here
                        /*else
                            CreateRecord(new DateTime(DateTime.Now.Year, month, day, i, 0, 0), i + 1, true, false);*/
                        counter++;
                    }
                }

                month++;
            }
            Assert.AreEqual(size + counter, Schedule.Records.Count);
            /*if (!secondRound)
                CreateNRecords(count, month, true);*/
        }

        /// <summary>
        ///   US005 Tests on UseCase to view all reservations
        /// </summary>
        [Test]
        public void CheckScheduleSingleUser()
        {
            var user = new User();
            user.Id = 1;
            var size = Schedule.Records.Count;
            var recordCount = (from record in Schedule.Records where record.User.Id == 1 select record).Count();

            CreateRecordWithDialogueClass(user, "3", "September", "19:00", "22:00");
            CreateRecordWithDialogueClass(user, "3", "September", "12:00", "13:00");
            CreateRecordWithDialogueClass(user, "3", "September", "10:00", "12:00");
            CreateRecordWithDialogueClass(user, "3", "September", "13:00", "17:00");
            CreateRecordWithDialogueClass(user, "3", "September", "17:00", "19:00");
            Assert.AreEqual(size + 5, Schedule.Records.Count);
            var records = from record in Schedule.Records where record.User.Id == 1 select record;
            Assert.AreEqual(recordCount + 5, records.Count());

            CreateRecordWithDialogueClass(user, "3", "September", "16:00", "20:00");
            Assert.AreEqual(size + 5, Schedule.Records.Count);
            records = from record in Schedule.Records where record.User.Id == 1 select record;
            Assert.AreEqual(recordCount + 5, records.Count());
        }
        
        [Test]
        public void CheckScheduleMultipleWithOverlap()
        {
            var user = new User();
            user.Id = 1;
            var size = Schedule.Records.Count;
            CreateRecordWithDialogueClass(user, "4", "September", "19:00", "22:00");

            var user2 = new User();
            user2.Id = 2;
            CreateRecordWithDialogueClass(user, "4", "September", "18:00", "20:00");

            var records1 = from record in Schedule.Records where record.User.Id == 1 select record;
            var records2 = from record in Schedule.Records where record.User.Id == 2 select record;

            Assert.AreEqual(size + 1, Schedule.Records.Count);
            Assert.AreEqual(1, records1.Count());
            Assert.AreEqual(0, records2.Count());
        }

        /// <summary>
        ///   US001 Tests on UseCase to update a reservation
        /// </summary>
        [Test]
        public void UpdateRecordInModel()
        {
            var user = new User();
            var size = Schedule.Records.Count;
            CreateRecordWithDialogueClass(user, "1", "August", "19:00", "22:00");

            Assert.AreEqual(size + 1, Schedule.Records.Count);
            StringAssert.AreEqualIgnoringCase("2019-08-01 19:00:00",
                Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));

            RecordModel.UpdateRecord(Schedule.Records[size], user, 18, 22);

            Assert.AreEqual(size + 1, Schedule.Records.Count);
            StringAssert.AreEqualIgnoringCase("2019-08-01 18:00:00",
                Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        ///  US003 Tests on UseCase to delete a record
        /// </summary>
        [Test]
        public void DeleteRecordFromModel()
        {
            var user = new User();
            var size = Schedule.Records.Count;
            CreateRecordWithDialogueClass(user, "2", "August", "19:00", "22:00");
            Assert.AreEqual(size + 1, Schedule.Records.Count);
            RecordModel.DeleteRecord(Schedule.Records[size]);
            Assert.AreEqual(size, Schedule.Records.Count);
        }

        [Test]
        public void RecordFromUserString()
        {
            var user = new User();
            var record = RecordModel.CreateRecord(user, 3, 8, 19, 22);
            TestFindRecordFromUserString(record);
            TestFindRecordFromUserString(RecordModel.CreateRecord(user, 3, 8, 17, 19));
        }

        private void TestFindRecordFromUserString(Record testRecord)
        {
            var size = Schedule.Records.Count;
            var text = testRecord.FromTime.ToString("dd MMMM, HH:mm") + "—" + testRecord.ToTime.Hour + ":00";
            CreateRecordWithDialogueClass(testRecord.User, testRecord.FromTime.ToString("dd"),
                testRecord.FromTime.ToString("MMMM"), testRecord.FromTime.ToString("HH:mm"),
                testRecord.ToTime.ToString("HH:mm"));
            Assert.AreEqual(size + 1, Schedule.Records.Count);
            var record = RecordModel.FindRecordByUserString(text);
            Assert.NotNull(record);
            StringAssert.AreEqualIgnoringCase(text,
                record.FromTime.ToString("dd MMMM, HH:mm") + "—" + record.ToTime.Hour + ":00");
        }

        private static void CreateRecordWithDialogueClass(User user, string day, string month, string startTime,
            string endTime)
        {
            var recordCreator = new CreateRecordDialogue(null);
            recordCreator.ProcessMonth(month);
            recordCreator.ProcessDay(day);
            recordCreator.ProcessTime(startTime, true);
            recordCreator.ProcessTime(endTime, false);
            recordCreator.ProcessApprove("Approve");
            recordCreator.Create(user);
        }
        static object[] dateObject =
        {
            new object[] { "1", "August", "19:00", "22:00" },
            new object[] { "2", "August", "19:00", "22:00" },
            new object[] { DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "19:00", "22:00" }
        };
        
        [TestCase("1", "April", "19:00", "22:00")] 
        [TestCase("2", "April", "19:00", "22:00")] 
        [TestCase("3", "April", "19:00", "22:00")]
        [Test, TestCaseSource("dateObject")]
        public static void CreateRecordWithDialogueClassPut(string day, string month, string startTime,
            string endTime)
        {
            var user = new User();
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