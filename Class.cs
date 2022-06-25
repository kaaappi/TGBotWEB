using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TGBot.Model;
using tryWeb.Model;

namespace tryWeb.TG_bot
{
    public class Class
    {
        TelegramBotClient botClient = new TelegramBotClient(Constants.ApiKey);
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
            Console.ReadKey();
        }
        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПI:\n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public string Answ = "USD";

        public List<long> ID = new List<long>();
        public List<string> Value = new List<string>();

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            var message = update.Message;
            WebClient webClient = new WebClient();
            AsyncButtons asyncButtons = new AsyncButtons();

            if (update?.Type == UpdateType.CallbackQuery)
            {
                await HandlerCallbackQuery(botClient, update.CallbackQuery);
            }
            else
                if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat, "Виберіть команду /keyboard");

                List<string> strings = new List<string>();
                var InDB = JsonConvert.SerializeObject(strings);
                var ForDB = new ModelForDB
                {
                    UserID = message.From.Id.ToString(),
                    MarketName = InDB
                };
                var json1 = JsonConvert.SerializeObject(ForDB);
                var data = new StringContent(json1, Encoding.UTF8, "application/json");

                var url = $"{Constants.adressMyAPI}/GetCourse/AddFavs";
                using var client = new HttpClient();

                var response = await client.PostAsync(url, data);
                return;
            }
            else
                if (message.Text == "/keyboard")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new
                    (
                    new[]
                        {
                        new KeyboardButton [] { "Курс гривні до іноземних валют"},
                        new KeyboardButton [] { "Курс криптовалют"}
                        }
                    )
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: replyKeyboardMarkup);
                return;
            }
            else
                if (message.Text == "Курс гривні до іноземних валют")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new
                    (
                    new[]
                        {
                        new KeyboardButton [] { "Долар США", "Євро"},
                        new KeyboardButton [] { "Фунт стерлінгів", "Злотий" }
                        }
                    )
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть валюту:", replyMarkup: replyKeyboardMarkup);
                return;
            }
            else
                if (message.Text == "Долар США")
            {

                Answ = "USD";
                await asyncButtons.ButtonsAsync(botClient, message);
                return;
            }
            else
                if (message.Text == "Євро")
            {
                Answ = "EUR";
                await asyncButtons.ButtonsAsync(botClient, message);
                return;
            }
            else
                if (message.Text == "Фунт стерлінгів")
            {
                Answ = "GBP";
                await asyncButtons.ButtonsAsync(botClient, message);

                return;
            }
            else
                if (message.Text == "Злотий")
            {
                Answ = "PLN";
                await asyncButtons.ButtonsAsync(botClient, message);

                return;
            }
            else
                if (message.Text == "Вчора")
            {

                string Yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                var json = webClient.DownloadString($"{Constants.adressMyAPI}/GetCourseDate/{Answ}/{Yesterday}");
                var result = JsonConvert.DeserializeObject<List<CourseResponse>>(json);
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Станом на: {result.FirstOrDefault().Exchangedate}\nКурс {result.FirstOrDefault().Cc} до гривні становить: *{result.FirstOrDefault().Rate}*", parseMode: ParseMode.Markdown);

                return;


            }
            else
            if (message.Text == "Сьогодні")
            {
                string Today = DateTime.Now.ToString("yyyyMMdd");

                var json = webClient.DownloadString($"{Constants.adressMyAPI}/GetCourseDate/{Answ}/{Today}");
                var result = JsonConvert.DeserializeObject<List<CourseResponse>>(json);
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Станом на: {result.FirstOrDefault().Exchangedate}\nКурс {result.FirstOrDefault().Cc} до гривні становить: *{result.FirstOrDefault().Rate}*", parseMode: ParseMode.Markdown);

                return;
            }
            else
                if (message.Text == "Своя дата")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Відправте дату у вигляді yyyyMMdd, де yyyy - рік, MM - місяць, dd - день:", replyMarkup: new ForceReplyMarkup { Selective = true });
                return;
            }
            else
                if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Відправте дату у вигляді yyyyMMdd, де yyyy - рік, MM - місяць, dd - день:"))
            {
                string Day = message.Text;
                var json = webClient.DownloadString($"{Constants.adressMyAPI}/GetCourseDate/{Answ}/{Day}");
                var result = JsonConvert.DeserializeObject<List<CourseResponse>>(json);
                if (result.FirstOrDefault().Cc == null)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Некоректна дата", parseMode: ParseMode.Markdown);
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Станом на: {result.FirstOrDefault().Exchangedate}\nКурс {result.FirstOrDefault().Cc} до гривні становить: *{result.FirstOrDefault().Rate}*", parseMode: ParseMode.Markdown);

                return;
            }
            else
                if (message.Text == "Курс криптовалют")
            {
                InlineKeyboardMarkup keyboardMarkup =
                        (
                            new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Binance", $"binance"),
                                    InlineKeyboardButton.WithCallbackData("Kucoin", $"kucoin")
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Gdax", $"gdax"),
                                    InlineKeyboardButton.WithCallbackData("Dcoin", $"dcoin")
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Huobi", $"huobi"),
                                    InlineKeyboardButton.WithCallbackData("Gate", $"gate")
                                },
                                new[]
                                {

                                    InlineKeyboardButton.WithCallbackData("AAX", $"aax"),
                                    InlineKeyboardButton.WithCallbackData("Coinzoom", $"coinzoom")
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Kraken", $"kraken"),
                                    InlineKeyboardButton.WithCallbackData("Lbank", $"lbank")
                                }
                            }
                        );
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть криптобіржу:", replyMarkup: keyboardMarkup);
                return;
            }

            else
                if (message.Text == "/add")
            {
                InlineKeyboardMarkup keyboardMarkup =
                        (
                            new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Binance", $"Binance"),
                                    InlineKeyboardButton.WithCallbackData("Kucoin", $"Kucoin")
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Gdax", $"Gdax"),
                                    InlineKeyboardButton.WithCallbackData("Dcoin", $"Dcoin")
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Huobi", $"Huobi"),
                                    InlineKeyboardButton.WithCallbackData("Gate", $"Gate")
                                },
                                new[]
                                {

                                    InlineKeyboardButton.WithCallbackData("AAX", $"AAX"),
                                    InlineKeyboardButton.WithCallbackData("Bitfinex", $"Bitfinex")
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Kraken", $"Kraken"),
                                    InlineKeyboardButton.WithCallbackData("Lbank", $"Lbank")
                                }
                            }
                        );

                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть криптобіржу для додавання в Favs:", replyMarkup: keyboardMarkup);

                return;
            }
            else
                if (message.Text == "/myfavs")
            {
                var json = webClient.DownloadString($"{Constants.adressMyAPI}/GetCourse/MarketsFromDBByID/{message.From.Id}");
                var result = JsonConvert.DeserializeObject<List<string>>(json);
                if (result.Count == 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"До списку обраного ще нічого не додано", parseMode: ParseMode.Markdown);

                }
                for (int i = 0; i < result.Count; i++)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Криптобіржа: {result[i]}", parseMode: ParseMode.Markdown);

                }
                return;
            }

            else
                if (message.Text == "/delete")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Відправте назву криптобіржі, яку хочете видалити зі свого списку обраного", replyMarkup: new ForceReplyMarkup { Selective = true });
                return;


            }
            else
                if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Відправте назву криптобіржі, яку хочете видалити зі свого списку обраного"))
            {
                var json = webClient.DownloadString($"{Constants.adressMyAPI}/GetCourse/MarketsFromDBByID/{message.From.Id}");
                var result = JsonConvert.DeserializeObject<List<string>>(json);
                if (result.Contains(message.Text))
                {
                    string MarketToDelete = message.Text;
                    var url = $"{Constants.adressMyAPI}/GetCourse/DeleteFavs/{message.From.Id}/{MarketToDelete}";
                    using var client = new HttpClient();
                    await client.DeleteAsync(url);
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Кріптобіржу {MarketToDelete} видалено із списку обраного");
                    return;


                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Некоректні дані");
                    return;
                }

            }
            else
                if (message.Text == "/deleteall")
            {
                var url = $"{Constants.adressMyAPI}/GetCourse/DeleteFavs/{message.From.Id}/all";
                using var client = new HttpClient();
                await client.DeleteAsync(url);
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Кріптобіржу видалено із списку обраного");

            }
        }

        private async Task HandlerCallbackQuery(ITelegramBotClient botClient, CallbackQuery? callbackQuery)
        {

            WebClient webClient = new WebClient();
            if (callbackQuery.Message.Text == "Виберіть криптобіржу:")
            {
                var json = webClient.DownloadString($"{Constants.adressMyAPI}/GetCourse/{callbackQuery.Data}");

                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                var result = JsonConvert.DeserializeObject<ModelCoinForBOT>(json);
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Криптобіржа: _{callbackQuery.Data}_" +
                    $"\nBitcoin до USD - *{result.Course}*\n", parseMode: ParseMode.Markdown);
                return;
            }
            if (callbackQuery.Message.Text == "Виберіть криптобіржу для додавання в Favs:")
            {
                var json = webClient.DownloadString($"{Constants.adressMyAPI}/GetCourse/MarketsFromDBByID/{callbackQuery.From.Id}");
                var result = JsonConvert.DeserializeObject<List<string>>(json);

                result.Add(callbackQuery.Data);
                result.Distinct();
                var DATAToDB = JsonConvert.SerializeObject(result);
                var ForDB = new ModelForDB
                {
                    UserID = callbackQuery.From.Id.ToString(),
                    MarketName = DATAToDB
                };
                var json1 = JsonConvert.SerializeObject(ForDB);
                var data = new StringContent(json1, Encoding.UTF8, "application/json");

                var url = $"{Constants.adressMyAPI}/GetCourse/AddFavs";
                using var client = new HttpClient();

                var response = await client.PostAsync(url, data);
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Біржу {callbackQuery.Data} додано до списку обраного ✅", showAlert: false);
                response.EnsureSuccessStatusCode();
                return;
            }
        }
    }


    public class AsyncButtons
    {
        public async Task ButtonsAsync(ITelegramBotClient botClient, Message message)
        {

            ReplyKeyboardMarkup replyKeyboardMarkupForChose = new
               (
               new[]
                   {
                        new KeyboardButton [] { "Вчора", "Сьогодні"},
                        new KeyboardButton [] { "Своя дата"}
                   }
               )
            {
                ResizeKeyboard = true
            };
            await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть дату для отримання курсу:", replyMarkup: replyKeyboardMarkupForChose);
            return;
        }
    }
}

