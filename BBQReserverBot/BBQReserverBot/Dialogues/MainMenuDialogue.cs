using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using BBQReserverBot.Model;

namespace BBQReserverBot.Dialogues
{
    public class MainMenuDialogue : AbstractDialogue
    {
        public MainMenuDialogue(Func<string, IReplyMarkup, Task<bool>> sendMessege) : base(sendMessege) { }
        private static string _menueMessage = "What do you like to do?";

        class MyScheduleDialogue2 : AbstractDialogue
        {
            public MyScheduleDialogue2(Func<string, IReplyMarkup, Task<bool>> onMessage) : base(onMessage) { }
            public async override Task<AbstractDialogue> OnMessage(MessageEventArgs args)
            {

                var records = from record in Schedule.Records where record.User.Id == args.Message.From.Id select record;
                _sendMessege("You have the following reservations:",
                             (ReplyKeyboardMarkup)
                             records
                             .Select(x => new []{x.FromTime.ToString("dd MMMM, hh:mm") + "—" + x.ToTime.Hour + ":00"})
                             .ToArray());

                var md = new MainMenuDialogue(_sendMessege);
                return md;
            }
        }

        public async override Task<AbstractDialogue> OnMessage(MessageEventArgs args)
        {
            var msgText = args.Message.Text;
            switch (msgText)
            {
                case "My schedule":
                    {
                        var dialogue = new MyScheduleDialogue2(_sendMessege);
                        return await dialogue.OnMessage(args);
                    }
                    break;
                case "Create a new reservation":
                    {
                        var dialogue = new CreateRecordDialogue(_sendMessege);
                        return await dialogue.OnMessage(args);
                    }
                    break;
                default :
                    {
                        ReplyKeyboardMarkup markup = new[]
                        {
                            "Create a new reservation",
                            //"Update or Remove an existing reservation",
                            "My schedule"
                        };
                        await _sendMessege(_menueMessage, markup);
                        return this;
                    }
                    break;
            }
        }
    }
}
