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
        public StartDialogue(Func<string, IReplyMarkup, bool> sendMessege) : base(sendMessege) { }
        private static string _helloMesege = "BBQReserver software";
        public override AbstractDialogue OnMessage(MessageEventArgs args)
        {
            ReplyKeyboardMarkup markup = new[]{"Get Started" };
            _sendMessege.Invoke(_helloMesege, markup);
            return new MainMenuDialogue(_sendMessege);
        }
    }
}
