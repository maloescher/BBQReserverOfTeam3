using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBQReserverBot.Model
{
    public class TelegramUser
    {
        public Guid Id { get; set; }
        public Telegram.Bot.Types.User UserInfo { get; set; }
}
}
