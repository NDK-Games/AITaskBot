using System.Net;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TaskManager;

public interface IMessageUpdateRouter
{
    Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct);
    Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct);
}

public class MessageUpdateHandler : IMessageUpdateRouter
{
    private readonly CommandDispatcher _dispatcher;
    private readonly IReportHandler _reportHandler;

    public MessageUpdateHandler(CommandDispatcher dispatcher, IReportHandler reportHandler)
    {
        _dispatcher = dispatcher;
        _reportHandler = reportHandler;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message is not { } message || message.Text is null || message.From == null) return;

        var text = message.Text.Trim();
        var account = DatabaseHelper.GetAccountByTelegramId(message.From.Id.ToString());

        if (text.StartsWith("/"))
        {
            var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string cmd = parts[0];
            string arg = parts.Length > 1 ? parts[1].Trim() : "";

            var ctx = new CommandContext(bot, message, ct, account, cmd, arg);
            if (!await _dispatcher.DispatchAsync(ctx))
            {
                await bot.SendTextMessageAsync(message.Chat.Id, "Неизвестная команда. Используйте /help.", cancellationToken: ct);
            }
            return;
        }

        await _reportHandler.ProcessAsync(bot, message, account, ct);
    }

    public Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        Log.E(exception, "Ошибка при обработке обновления Telegram.");
        return Task.CompletedTask;
    }
}
