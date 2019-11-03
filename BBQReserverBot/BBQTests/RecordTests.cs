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
        static object[] deletRecordTestObjects =
        {
            new object[] {new User(), 3, 8, 19, 22, true},
            new object[] {new User(), 3, 8, 19, 22, false},
        };

        [Test, TestCaseSource("deletRecordTestObjects")]
        public void DeleteRecordTest(User user, int selectedDay, int selectedMonth, int selectedStart, int selectedEnd, bool exist)
        {
            var size = Schedule.Records.Count;            
            var record = new Record(user, selectedDay, selectedMonth, selectedStart, selectedEnd);
            
            //Record is exist or not
            if (exist)
            {
                //Add record
                RecordModel.CreateRecord(user, selectedDay, selectedMonth, selectedStart, selectedEnd, exist);
                Assert.AreEqual(size + 1, Schedule.Records.Count);
                
                //Select record from Record List
                var selectedRecord =
                    RecordModel.FindRecordByUserString(record.FromTime.ToString("dd MMMM, HH:mm") + "—" +
                                                       record.ToTime.Hour + ":00");
                
                //Delete selected record
                RecordModel.DeleteRecord(selectedRecord);
                Assert.AreEqual(size, Schedule.Records.Count);
            }
            else
            {
                //Select record from Record List
                var selectedRecord =
                    RecordModel.FindRecordByUserString(record.FromTime.ToString("dd MMMM, HH:mm") + "—" +
                                                       record.ToTime.Hour + ":00");
                //Delete non-exist record
                RecordModel.DeleteRecord(selectedRecord);
                Assert.AreEqual(size, Schedule.Records.Count);   
            }
        }

        static object[] findRecordTestObjects =
        {
            new object[] {new User(), 3, 8, 19, 22, true},
            new object[] {new User(), 3, 8, 19, 22, false},
            new object[] {new User(), 4, 8, 19, 22, false},
            new object[] {new User(), 4, 8, 19, 22, true},
        };
        [Test, TestCaseSource("findRecordTestObjects")]
        public void FindRecordTest(User user, int selectedDay, int selectedMonth, int selectedStart, int selectedEnd, bool exist)
        {
            var size = Schedule.Records.Count;            
            var testRecord = new Record(user, selectedDay, selectedMonth, selectedStart, selectedEnd);
            var text = testRecord.FromTime.ToString("dd MMMM, HH:mm") + "—" + testRecord.ToTime.Hour + ":00";
            
            //Record is exist or not
            if (exist)
            {
                //Add record
                RecordModel.CreateRecord(user, selectedDay, selectedMonth, selectedStart, selectedEnd, exist);
                Assert.AreEqual(size + 1, Schedule.Records.Count);
                
                //Select record from Record List
                var record = RecordModel.FindRecordByUserString(text);
                Assert.NotNull(record);
                StringAssert.AreEqualIgnoringCase(text,
                    record.FromTime.ToString("dd MMMM, HH:mm") + "—" + record.ToTime.Hour + ":00");
            }
            else
            {
                var record = RecordModel.FindRecordByUserString(text);
                Assert.Null(record);   
            }
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