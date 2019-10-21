using BBQReserverBot.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBQReserverBot.Dialogues
{
    public class CreateRecordDialogue : AbstractDialogue
    {
        public CreateRecordDialogue(Func<string, IReplyMarkup, Task<bool>> onMessage) : base(onMessage) { }
        private Record record;
        public async override Task<AbstractDialogue> OnMessage(MessageEventArgs args)
        {
            var msgText = args.Message.Text;
            switch (msgText)
            {
                case "08:00":
                case "09:00":
                case "10:00":
                case "11:00":
                case "12:00":
                case "13:00":
                case "14:00":
                case "15:00":
                case "16:00":
                case "17:00":
                case "18:00":
                case "19:00":
                case "20:00":
                case "21:00":
                    {
                        if (!ProcessTime(msgText))
                        {
                            ReplyKeyboardMarkup markup = new[]
                                    {
                                    new[]{"08:00" },
                                    new[]{"09:00" },
                                    new[]{"10:00" },
                                    new[]{"11:00" },
                                    new[]{"12:00" },
                                    new[]{"13:00" },
                                    new[]{"14:00" },
                                    new[]{"15:00" },
                                    new[]{"16:00" },
                                    new[]{"17:00" },
                                    new[]{"18:00" },
                                    new[]{"19:00" },
                                    new[]{"20:00" },
                                    new[]{"21:00" },
                                };
                            await _sendMessege("Select new starting time", markup);
                            return this;
                        }
                        else
                        {
                            ReplyKeyboardMarkup markup = new[]
                           {
                                    new[]{"Approve" },
                                    new[]{"I've changed my mind" },
                                };
                            await _sendMessege("Great! Just approve your reservation and thats it!", markup);
                            return this;
                        }
                    }
                    break;
                case "January":
                case "February":
                case "March":
                case "April":
                case "May":
                case "Jun":
                case "July":
                case "August":
                case "September":
                case "October":
                case "November":
                case "December":
                    {
                        ProcessMonth(msgText);
                        await _sendMessege("Write the day in numbers it shuld be bigger than todays date", new ReplyKeyboardRemove());
                        return this;
                    }
                    break;
                case "Create a new reservation":
                    {
                        ReplyKeyboardMarkup markup = new[]
                        {
                            new[]{"January" },
                            new[]{"February" },
                            new[]{"March" },
                            new[]{"April" },
                            new[]{"May" },
                            new[]{"Jun" },
                            new[]{"July" },
                            new[]{"August" },
                            new[]{"September" },
                            new[]{"October" },
                            new[]{"November" },
                            new[]{"December" },
                        };
                        await _sendMessege("Select month for the reservation", markup);
                        record = new Record();
                        record.User = args.Message.From;
                        return this;
                    }
                    break;
                case "Approve":
                    {
                        ProcessMonth(msgText);
                        await _sendMessege("Yay your reservation is approved! Have a great BBBQing", new ReplyKeyboardRemove());
                        var dialog = new MainMenuDialogue(_sendMessege);
                        return await dialog.OnMessage(args);
                    }
                    break;
                case "I've changed my mind":
                    {
                        ProcessMonth(msgText);
                        await _sendMessege("Okay. See you next time!", new ReplyKeyboardRemove());
                        var dialog = new MainMenuDialogue(_sendMessege);
                        return await dialog.OnMessage(args);
                    }
                    break;
                default:
                    {
                        if(int.TryParse(msgText, out int day))
                        {
                            if(day>0 && day < 32)
                            {
                                ProcessDay(msgText);
                                ReplyKeyboardMarkup markup = new[]
                                {
                                    new[]{"08:00" },
                                    new[]{"09:00" },
                                    new[]{"10:00" },
                                    new[]{"11:00" },
                                    new[]{"12:00" },
                                    new[]{"13:00" },
                                    new[]{"14:00" },
                                    new[]{"15:00" },
                                    new[]{"16:00" },
                                    new[]{"17:00" },
                                    new[]{"18:00" },
                                    new[]{"19:00" },
                                    new[]{"20:00" },
                                    new[]{"21:00" },
                                };
                                await _sendMessege("Select new starting time", markup);
                                return this;
                            }
                            else
                            {
                                ProcessMonth(msgText);
                                await _sendMessege("Can't recognise day", new ReplyKeyboardRemove());
                                return this;
                            }

                        }
                        else
                        {
                            ProcessMonth(msgText);
                            await _sendMessege("Smth went wrong", new ReplyKeyboardRemove());
                            var dialog = new MainMenuDialogue(_sendMessege);
                            return await dialog.OnMessage(args);
                        }
                    }
                    break;
            }
        }

        private void ProcessMonth(string month)
        {
            record.FromTime = new DateTime(DateTime.Now.Year, DateTime.ParseExact(month, "MMMM", CultureInfo.CurrentCulture).Month, 1);
        }

        private bool ProcessTime(string time)
        {
            if (record.FromTime.Hour == 0)
            {
                record.FromTime.AddHours(DateTime.ParseExact(time, "hh:mm", CultureInfo.CurrentCulture).Hour);
                if (DateTime.Now > record.FromTime)
                {
                    record.FromTime.AddYears(1);
                }
                return false;
            }
            else
            {
                record.ToTime = record.FromTime;
                record.ToTime.AddHours(DateTime.ParseExact(time, "hh:mm", CultureInfo.CurrentCulture).Hour);
                if (DateTime.Now > record.FromTime)
                {
                    record.FromTime.AddYears(1);
                }
                return true;
            }
        }

        private void ProcessDay(string day)
        {
            /*
            var firstDayOfMonth = new DateTime(record.FromTime.Year, record.FromTime.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);*/

            record.FromTime.AddDays(DateTime.ParseExact(day, "dd", CultureInfo.CurrentCulture).Day - 1);
        }
    }
}
