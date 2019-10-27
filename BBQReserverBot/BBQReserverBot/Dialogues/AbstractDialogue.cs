using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBQReserverBot.Dialogues
{
    public abstract class AbstractDialogue
    {
        protected Func<string, IReplyMarkup, Task<bool>> _sendMessege;

        public AbstractDialogue(Func<string, IReplyMarkup, Task<bool>> sendMessage)
        {
            _sendMessege = sendMessage;
        }
        public abstract Task<AbstractDialogue> OnMessage(MessageEventArgs args);
    }
}
