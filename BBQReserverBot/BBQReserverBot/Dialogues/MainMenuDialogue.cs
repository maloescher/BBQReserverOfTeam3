using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBQReserverBot.Dialogues
{
    public class MainMenuDialogue : AbstractDialogue
    {
        public MainMenuDialogue(Func<string, IReplyMarkup, Task<bool>> sendMessege) : base(sendMessege) { }
        private static string _menueMessage = "What do you like to do?";

        public async override Task<AbstractDialogue> OnMessage(MessageEventArgs args)
        {
            var msgText = args.Message.Text;
            switch (msgText)
            {
                case "View the schedule":
                    {
                        var dialogue = new MyScheduleDialogue(_sendMessege);
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
                            //"Create a new reservation",
                            //"Update or Remove an existing reservation",
                            "View the schedule"
                        };
                        await _sendMessege(_menueMessage, markup);
                        return this;
                    }
                    break;
            }
        }
    }
}
