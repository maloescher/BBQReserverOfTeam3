using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        public void afterEach()
        {
            Schedule.Records.Clear();
        }

        /// <summary>
        ///   US002 Tests on UseCase creation of record
        /// </summary>
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

        [Test]
        public void CreateInvalidRecord()
        {
            var user = new User();
            var size = Schedule.Records.Count;
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CreateRecordWithDialogueClass(user, "1", "June", "19:00", "23:00"));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CreateRecordWithDialogueClass(user, "1", "June", "6:00", "12:00"));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CreateRecordWithDialogueClass(user, "1", "June", "6:00", "23:00"));
            Assert.AreEqual(size, Schedule.Records.Count);
        }

        [TestCase(30, 5)]
        [TestCase(600, 6)]
        [TestCase(65, 4)]
        public void CreateNRecords(int count, int month)
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
                        RecordModel.CreateRecord(user, day, month, i, i + 1, true);
                        counter++;
                    }
                }

                month++;
            }

            Assert.AreEqual(size + counter, Schedule.Records.Count);
        }

        [Test]
        public void TryToCreateOverlappingRecord()
        {
            var user = new User();
            var size = Schedule.Records.Count;
            CreateRecordWithDialogueClass(user, "2", "September", "19:00", "22:00");

            Assert.AreEqual(size + 1, Schedule.Records.Count);
            StringAssert.AreEqualIgnoringCase("2019-09-02 19:00:00",
                Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));

            var user2 = new User();
            user2.Id = 2;
            CreateRecordWithDialogueClass(user2, "2", "September", "18:00", "22:00");

            Assert.AreEqual(size + 1, Schedule.Records.Count); // Not +2 as we expect not to write into Schedule
            StringAssert.AreEqualIgnoringCase("2019-09-02 19:00:00",
                Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        
        /// <summary>
        ///   US005 Tests on UseCase to view all reservations
        /// </summary>
        static object[] singleUserRecords =
        {
            new object[] {
                new User(),
                1,
                new TestRecord ( DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "10:00", "12:00" ),
                new TestRecord ( DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "12:00", "13:00" )
            }
        };
        
        [TestCase, TestCaseSource("singleUserRecords")]
        public void CheckScheduleSingleUser(User user, int userId, TestRecord firstRecord, TestRecord secondRecord)
        {
            user.Id = userId;
            firstRecord.setUser(user);
            secondRecord.setUser(user);
            var size = Schedule.Records.Count;
            var userRecordCount = (from record in Schedule.Records where record.User.Id == userId select record).Count();
            
            CreateRecordWithDialogueClass(firstRecord);
            CreateRecordWithDialogueClass(secondRecord);
            Assert.AreEqual(size + 2, Schedule.Records.Count);
            var userSchedule = from record in Schedule.Records where record.User.Id == userId select record;
            Assert.AreEqual(userRecordCount + 2, userSchedule.Count());
        }

        static object[] multipleUserRecords =
        {
            new object[] {
                new User(),
                1,
                new User(),
                2,
                new TestRecord ( DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "19:00", "22:00" ),
                new TestRecord ( DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "18:00", "20:00" )
            }
        };
        
        [TestCase, TestCaseSource("multipleUserRecords")]
        public void CheckOverlapScheduleForMultipleUsers(User firstUser, int firstId, User secondUser, int secondId, TestRecord firstRecord, TestRecord secondRecord)
        {
            firstUser.Id = firstId;
            var size = Schedule.Records.Count;
            firstRecord.setUser(firstUser);
            CreateRecordWithDialogueClass(firstRecord);
            
            secondUser.Id = secondId;
            secondRecord.setUser(secondUser);
            CreateRecordWithDialogueClass(secondRecord);

            var firstUserSchedule = from record in Schedule.Records where record.User.Id == firstId select record;
            var secondUserSchedule = from record in Schedule.Records where record.User.Id == secondId select record;

            Assert.AreEqual(size + 1, Schedule.Records.Count);
            Assert.AreEqual(1, firstUserSchedule.Count());
            Assert.AreEqual(0, secondUserSchedule.Count());
        }

        /// <summary>
        ///   US001 Tests on UseCase to update a reservation
        /// </summary>
        static object[] modelRecord =
        {
            new object[] {
                new User(),
                new TestRecord ( DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "19:00", "22:00" ),
                DateTime.Now.ToString("yyyy-MM-dd"),
                18,
                22
            }
        };
        
        [TestCase, TestCaseSource("modelRecord")]
        public void UpdateRecordInModel(User user, TestRecord record, String recordDate, int newStartTime, int newEndTime)
        {
            var size = Schedule.Records.Count;
            record.setUser(user);
            
            CreateRecordWithDialogueClass(record);
            Assert.AreEqual(size + 1, Schedule.Records.Count);
            
            StringAssert.AreEqualIgnoringCase(recordDate + " " + record.startTime + ":00",
                Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));

            RecordModel.UpdateRecord(Schedule.Records[size], user, newStartTime, newEndTime);

            Assert.AreEqual(size + 1, Schedule.Records.Count);
            
            StringAssert.AreEqualIgnoringCase(recordDate + " " + newStartTime + ":00:00",
                Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        
        static object[] modelRecordWithOverlap =
        {
            new object[] {
                new User(),
                new TestRecord ( DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "19:00", "22:00" ),
                new TestRecord ( DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "11:00", "12:00" ),
                DateTime.Now.ToString("yyyy-MM-dd"),
                11,
                14
            }
        };
        
        [TestCase, TestCaseSource("modelRecordWithOverlap")]
        public void UpdateRecordInModelWithOverlap(User user, TestRecord firstRecord, TestRecord secondRecord, String recordDate, int newStartTime, int newEndTime)
        {
            var size = Schedule.Records.Count;
            firstRecord.setUser(user);
            secondRecord.setUser(user);
            
            CreateRecordWithDialogueClass(firstRecord);
            CreateRecordWithDialogueClass(secondRecord);
            Assert.AreEqual(size + 2, Schedule.Records.Count);
            
            StringAssert.AreEqualIgnoringCase(recordDate + " " + firstRecord.startTime + ":00",
                Schedule.Records[size].FromTime.ToString("yyyy-MM-dd HH:mm:ss"));

            RecordModel.UpdateRecord(Schedule.Records[size], user, newStartTime, newEndTime);

            Assert.AreEqual(size + 2, Schedule.Records.Count);
            
            StringAssert.AreNotEqualIgnoringCase(recordDate + " " + newStartTime + ":00:00",
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
        
        private static void CreateRecordWithDialogueClass(TestRecord testRecord)
        {
            var recordCreator = new CreateRecordDialogue(null);
            recordCreator.ProcessMonth(testRecord.month);
            recordCreator.ProcessDay(testRecord.day);
            recordCreator.ProcessTime(testRecord.startTime, true);
            recordCreator.ProcessTime(testRecord.endTime, false);
            recordCreator.ProcessApprove("Approve");
            recordCreator.Create(testRecord.user);
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
        
        public class TestRecord
        {
            public User user;
            public string day;
            public string month;
            public string startTime;
            public string endTime;
            public TestRecord(string day, string month, string startTime,
                string endTime)
            {
                this.day = day;
                this.month = month;
                this.startTime = startTime;
                this.endTime = endTime;
            }

            public void setUser(User user)
            {
                this.user = user;
            }
        }
    }
}