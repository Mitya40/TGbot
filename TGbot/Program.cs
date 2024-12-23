using System;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
    static ITelegramBotClient botClient;

    static async Task Main()
    {
        botClient = new TelegramBotClient("token"); // Токен
        Console.WriteLine("Бот запущен");

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Настроим получения обновлений
        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            cancellationToken: cancellationToken
        );

        Console.WriteLine("Нажмите любую клавишу для остановки...");
        Console.ReadKey();

        cts.Cancel();
    }

    static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        // Обрабатываем только текстовые сообщения
        if (update.Type != UpdateType.Message || update.Message?.Type != MessageType.Text)
            return;

        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;

        if (messageText == "/start")
        {
            await bot.SendTextMessageAsync(chatId, "Привет! Напишите дату и время события в формате ГГГГ-ММ-ДД ЧЧ:ММ", cancellationToken: cancellationToken);
        }
        else
        {
            // Пробуем преобразовать сообщение в дату
            if (DateTime.TryParse(messageText, out DateTime eventTime))
            {
                // Запускаем задачу для отправки напоминания
                var delay = eventTime - DateTime.Now;
                if (delay > TimeSpan.Zero)
                {
                    await bot.SendTextMessageAsync(chatId, "Напоминание установлено! Я напомню вам об этом событии.", cancellationToken: cancellationToken);

                    // Ожидаем до времени события
                    await Task.Delay(delay, cancellationToken);

                    // Отправляем напоминание
                    await bot.SendTextMessageAsync(chatId, $"Напоминание: время события {eventTime}.", cancellationToken: cancellationToken);
                }
                else
                {
                    await bot.SendTextMessageAsync(chatId, "Время должно быть в будущем. Пожалуйста, укажите корректное время.", cancellationToken: cancellationToken);
                }
            }
            else
            {
                await bot.SendTextMessageAsync(chatId, "Неверный формат даты и времени. Пожалуйста, используйте формат ГГГГ-ММ-ДД ЧЧ:ММ", cancellationToken: cancellationToken);
            }
        }
    }

    static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Произошла ошибка: {exception}");
        return Task.CompletedTask;
    }
}
