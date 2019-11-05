using System;
using System.Collections.Generic;
using System.Linq;
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

        [TearDown]
        public void ClearRecordsAfterEachTest()
        {
            Schedule.Records.Clear();
        }

        /// <summary>
        ///   US002 Tests on UseCase creation of record
        /// </summary>
        /// 
        static object[] createRecordObject =
        {
            //todo bug found here
            new object[] {new DateTime(DateTime.Now.Year, 3, 20, 8, 0, 0), 13, true, false},
            new object[] {new DateTime(DateTime.Now.Year, 05, 01, 9, 0, 0), 12, true, false},
            new object[] {new DateTime(DateTime.Now.Year, 04, 01, 10, 0, 0), 12, true, false},
            new object[] {new DateTime(DateTime.Now.Year, 09, 01, 19, 0, 0), 22, true, false},
            new object[] {new DateTime(DateTime.Now.Year, 08, 01, 21, 0, 0), 22, true, false},
            new object[] {new DateTime(DateTime.Now.Year, 07, 01, 22, 0, 0), 22, true, false},
            new object[] {new DateTime(DateTime.Now.Year, 01, 01, 6, 0, 0), 23, false, true},
            new object[] {new DateTime(DateTime.Now.Year, 01, 02, 18, 0, 0), 23, false, true},
            new object[] {new DateTime(DateTime.Now.Year, 01, 03, 6, 0, 0), 22, false, true},
        };

        [Test, TestCaseSource("createRecordObject")]
        public void CreateRecord_AddValidRecord_RefuseInvalidRecord(DateTime startDateTime, int endTimeInt, bool valid,
            bool outOfRange)
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
            else
            {
                if (outOfRange)
                    Assert.Throws<ArgumentOutOfRangeException>(() =>
                        CreateRecordWithDialogueClass(user, day, month, startTime, endTime));
                else
                    Assert.AreEqual(size, Schedule.Records.Count);
            }
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
        public void CreateRecord_RefuseOverlappingRecord(DateTime startDateTime1, int endTimeInt1,
            DateTime startDateTime2,
            int endTimeInt2)
        {
            var size = Schedule.Records.Count;
            CreateRecord_AddValidRecord_RefuseInvalidRecord(startDateTime1, endTimeInt1, true, false);
            CreateRecord_AddValidRecord_RefuseInvalidRecord(startDateTime2, endTimeInt2, false, false);
            var size1 = Schedule.Records.Count;
            Assert.AreEqual(size + 1, Schedule.Records.Count);
        }

        [TestCase(30, 5, false)]
        [TestCase(600, 8, false)]
        [TestCase(65, 4, false)]
        public void CreateRecord_AddValidRecords(int count, int month, bool secondRound)
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
                        {
                            var testDate = new DateTime(DateTime.Now.Year, month, day, i, 0, 0);
                            var monthName = testDate.ToString("MMMM");
                            CreateRecordWithDialogueClass(user, day.ToString(), monthName, i + ":00", (i + 1) + ":00");
                        }
                        //todo bug found here
                        else
                            CreateRecord_AddValidRecord_RefuseInvalidRecord(
                                new DateTime(DateTime.Now.Year, month, day, i, 0, 0), i + 1, true, false);
                        counter++;
                    }
                }

                month++;
            }

            Assert.AreEqual(size + counter, Schedule.Records.Count);
            if (!secondRound)
                CreateRecord_AddValidRecords(count, month, true);
        }

        /// <summary>
        ///   US005 Tests on UseCase to view all reservations
        /// </summary>
        static object[] singleUserRecords =
        {
            new object[]
            {
                new User(),
                1,
                new TestRecord(DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "10:00", "12:00"),
                new TestRecord(DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "12:00", "13:00")
            },
        };

        [Test, TestCaseSource("singleUserRecords")]
        public void ScheduleRecords_GetForSingleUser(User user, int userId, TestRecord firstRecord,
            TestRecord secondRecord)
        {
            user.Id = userId;
            firstRecord.SetUser(user);
            secondRecord.SetUser(user);
            var size = Schedule.Records.Count;
            var userRecordCount =
                (from record in Schedule.Records where record.User.Id == userId select record).Count();

            CreateRecordWithDialogueClass(firstRecord);
            CreateRecordWithDialogueClass(secondRecord);
            Assert.AreEqual(size + 2, Schedule.Records.Count);
            var userSchedule = from record in Schedule.Records where record.User.Id == userId select record;
            Assert.AreEqual(userRecordCount + 2, userSchedule.Count());
        }

        static object[] multipleUserRecords =
        {
            new object[]
            {
                new User(), 1,
                new User(), 2,
                new TestRecord(DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "19:00", "22:00"),
                new TestRecord(DateTime.Now.ToString("dd"), DateTime.Now.ToString("MMMM"), "18:00", "20:00")
            },
        };

        [Test, TestCaseSource("multipleUserRecords")]
        public void ScheduleRecords_RefuseOverlappingRecord_GetForMultipleUsers(User firstUser, int firstId,
            User secondUser, int secondId,
            TestRecord firstRecord, TestRecord secondRecord)
        {
            firstUser.Id = firstId;
            var size = Schedule.Records.Count;
            firstRecord.SetUser(firstUser);

            var firstUserSchedule = from record in Schedule.Records where record.User.Id == firstId select record;
            var secondUserSchedule = from record in Schedule.Records where record.User.Id == secondId select record;
            var firstUserSize = firstUserSchedule.Count();
            var secondUserSize = secondUserSchedule.Count();

            CreateRecordWithDialogueClass(firstRecord);

            secondUser.Id = secondId;
            secondRecord.SetUser(secondUser);
            CreateRecordWithDialogueClass(secondRecord);

            firstUserSchedule = from record in Schedule.Records where record.User.Id == firstId select record;
            secondUserSchedule = from record in Schedule.Records where record.User.Id == secondId select record;

            Assert.AreEqual(size + 1, Schedule.Records.Count);
            Assert.AreEqual(firstUserSize + 1, firstUserSchedule.Count());
            Assert.AreEqual(secondUserSize, secondUserSchedule.Count());
        }


        static object[] findRecordTestObjects =
        {
            new object[] {new User(), 3, 8, 19, 22, true},
            new object[] {new User(), 3, 8, 19, 22, false},
            new object[] {new User(), 4, 8, 19, 22, false},
            new object[] {new User(), 4, 8, 19, 22, true},
        };

        //Tests end

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

            public void SetUser(User user)
            {
                this.user = user;
            }
        }
    }
}