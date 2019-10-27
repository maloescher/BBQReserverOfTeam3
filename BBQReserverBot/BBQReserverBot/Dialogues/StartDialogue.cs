using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBQReserverBot.Dialogues
{
    public class StartDialogue : AbstractDialogue
    {
        public StartDialogue(Func<string, IReplyMarkup, Task<bool>> sendMessege) : base(sendMessege) {

        }
        private static string _helloMesege = "Welcome to the BBQReserver bot! \nwhat can I do?\n" +
            " - Create reservations for the BBQ zone of Innopolis\n" +
            " - Monitor your reservations of the BBQ zone of Innopolis";
        public async void PrintInitialMessage()
        {
            ReplyKeyboardMarkup markup = new[] { "Get Started" };
            await _sendMessege.Invoke(_helloMesege, markup);
        }

        public async override  Task<AbstractDialogue> OnMessage(MessageEventArgs args)
        {
            var dialog = new MainMenuDialogue(_sendMessege);
            await dialog.OnMessage(args);
            return dialog;
        }
    }
}
