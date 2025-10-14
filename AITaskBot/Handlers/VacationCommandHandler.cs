using System.Collections.Concurrent;
using System.Globalization;
using TaskManager;
using Telegram.Bot;

public class VacationCommand : ICommand
{
    public bool CanHandle(string command) => string.Equals(command, "/vacation", StringComparison.OrdinalIgnoreCase);

    public async Task ExecuteAsync(CommandContext ctx)
    {
        // Команду может вызывать любой пользователь для себя
        if (ctx.Account == null)
        {
            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, "Аккаунт не найден.", cancellationToken: ctx.CancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(ctx.Argument))
        {
            await ctx.Bot.SendTextMessageAsync(
                ctx.Message.Chat.Id,
                "Укажите дату в формате DD.MM.YYYY.\nПример: /vacation 25.09.2025",
                cancellationToken: ctx.CancellationToken);
            return;
        }

        var arg = ctx.Argument.Trim();
        if (!DateOnly.TryParseExact(arg, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var until))
        {
            await ctx.Bot.SendTextMessageAsync(
                ctx.Message.Chat.Id,
                "Неверный формат даты. Используйте DD.MM.YYYY.\nПример: /vacation 25.09.2025",
                cancellationToken: ctx.CancellationToken);
            return;
        }

        // Сохраняем дату «до включительно»
        VacationRegistry.SetVacationUntil(ctx.Account.TelegramUserId, until);

        await ctx.Bot.SendTextMessageAsync(
            ctx.Message.Chat.Id,
            $"Ок! Напоминания отключены до {until:dd.MM.yyyy} включительно.",
            cancellationToken: ctx.CancellationToken);
    }
}

public static class VacationRegistry
{
    // Хранит дату, до которой действует «отпуск» (включительно), в локальной дате пользователя
    private static readonly ConcurrentDictionary<long, DateOnly> _vacations = new();

    public static void SetVacationUntil(long userId, DateOnly untilInclusive)
    {
        _vacations[userId] = untilInclusive;
    }

    public static bool TryGetVacationUntil(long userId, out DateOnly untilInclusive)
    {
        return _vacations.TryGetValue(userId, out untilInclusive);
    }

    public static bool IsOnVacation(long userId, DateOnly todayLocal)
    {
        return _vacations.TryGetValue(userId, out var untilInclusive) && todayLocal <= untilInclusive;
    }
}

