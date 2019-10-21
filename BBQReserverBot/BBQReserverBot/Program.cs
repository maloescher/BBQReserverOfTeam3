using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BBQReserverBot.Dialogues;
using BBQReserverBot.Model;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace BBQReserverBot
{
    public class Program
    {
        private static Dictionary<int, AbstractDialogue> users;
        private static readonly TelegramBotClient Bot = new TelegramBotClient("Your API key");
        public static void Main(string[] args)
        {
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            Bot.StopReceiving();
            // CreateWebHostBuilder(args).Build().Run();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            if (TryFindUser(messageEventArgs, out var user))
            {
                var dialog = await users[user.GetValueOrDefault()].OnMessage(messageEventArgs);
                users[user.GetValueOrDefault()] = dialog;
            }
            else
            {
                users.Add(messageEventArgs.Message.From.Id, new StartDialogue(async (string msg, IReplyMarkup markup) =>
                {
                    await Bot.SendTextMessageAsync(
                    messageEventArgs.Message.Chat.Id,
                    msg,
                    replyMarkup: markup);
                    return true;
                }));
                var dialog = await users[user.GetValueOrDefault()].OnMessage(messageEventArgs);
                users[user.GetValueOrDefault()] = dialog;
            }

        }
        private static bool TryFindUser(MessageEventArgs messageEventArgs, out int? user )
        {
            var us = from u in users.Keys where u == messageEventArgs.Message.From.Id select u;
            if(us.Count() > 0)
            {
                user = us.FirstOrDefault();
                return true;
            }
            else
            {
                user = null;
                return false;
            }
        }
        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await Bot.AnswerCallbackQueryAsync(
                callbackQuery.Id,
                $"Received {callbackQuery.Data}");

            await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                $"Received {callbackQuery.Data}");
        }

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                new InlineQueryResultLocation(
                    id: "1",
                    latitude: 40.7058316f,
                    longitude: -74.2581888f,
                    title: "New York")   // displayed result
                    {
                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 40.7058316f,
                            longitude: -74.2581888f)    // message if result is selected
                    },

                new InlineQueryResultLocation(
                    id: "2",
                    latitude: 13.1449577f,
                    longitude: 52.507629f,
                    title: "Berlin") // displayed result
                    {
                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 13.1449577f,
                            longitude: 52.507629f)   // message if result is selected
                    }
            };

            await Bot.AnswerInlineQueryAsync(
                inlineQueryEventArgs.InlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0);
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }


    //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    //WebHost.CreateDefaultBuilder(args)
    //  .UseStartup<Startup>();

    /*
var message = messageEventArgs.Message;

if (message == null || message.Type != MessageType.Text) return;

switch (message.Text.Split(' ').First())
{
    // send inline keyboard
    case "/inline":
        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        await Task.Delay(500); // simulate longer running task

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new [] // first row
            {
                InlineKeyboardButton.WithCallbackData("1.1"),
                InlineKeyboardButton.WithCallbackData("1.2"),
            },
            new [] // second row
            {
                InlineKeyboardButton.WithCallbackData("2.1"),
                InlineKeyboardButton.WithCallbackData("2.2"),
            }
        });

        await Bot.SendTextMessageAsync(
            message.Chat.Id,
            "Choose",
            replyMarkup: inlineKeyboard);
        break;

    // send custom keyboard
    case "/keyboard":
        ReplyKeyboardMarkup ReplyKeyboard = new[]
        {
            new[] { "1.1", "1.2" },
            new[] { "2.1", "2.2" },
        };

        await Bot.SendTextMessageAsync(
            message.Chat.Id,
            "Choose",
            replyMarkup: ReplyKeyboard);
        break;

    // request location or contact
    case "/request":
        var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
        {
            KeyboardButton.WithRequestLocation("Location"),
            KeyboardButton.WithRequestContact("Contact"),
        });

        await Bot.SendTextMessageAsync(
            message.Chat.Id,
            "Who or Where are you?",
            replyMarkup: RequestReplyKeyboard);
        break;

    default:
        const string usage = @"
Usage:
/inline   - send inline keyboard
/keyboard - send custom keyboard
/photo    - send a photo
/request  - request location or contact";

        await Bot.SendTextMessageAsync(
            message.Chat.Id,
            usage,
            replyMarkup: new ReplyKeyboardRemove());
        break;
}*/

}
