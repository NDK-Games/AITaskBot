using TaskManager;
using Telegram.Bot;

public class StatsCommand : ICommand
{
    private readonly DateRangeParser _parser;

    public StatsCommand(DateRangeParser parser) => _parser = parser;

    public bool CanHandle(string command) => string.Equals(command, "/stats", StringComparison.OrdinalIgnoreCase);

    public async Task ExecuteAsync(CommandContext ctx)
    {
        if (ctx.Account == null || ctx.Account.Role == Role.User)
        {
            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, "У вас недостаточно прав для этой команды.", cancellationToken: ctx.CancellationToken);
            return;
        }

        if (string.IsNullOrEmpty(ctx.Argument))
        {
            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, "Введите: /stats [ID] [DD.MM.YYYY-DD.MM.YYYY]\nлибо /stats DD.MM.YYYY-DD.MM.YYYY", cancellationToken: ctx.CancellationToken);
            return;
        }

        var tokens = ctx.Argument.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 2)
        {
            string targetUserId = tokens[0];
            string dateRange = tokens[1];

            if (!_parser.TryParse(dateRange, out var dateFrom, out var dateTo))
            {
                await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, "Неверный формат дат. Пример: /stats 123456789 01.02.2025-06.02.2025", cancellationToken: ctx.CancellationToken);
                return;
            }

            var allReports = DatabaseHelper.GetReportsByDateRange(dateFrom, dateTo);
            var userReports = allReports
                .Where(r => r.UserId.ToString() == targetUserId)
                .OrderBy(r => r.ReportDate)
                .ToList();

            if (!userReports.Any())
            {
                await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, $"Отчётов пользователя {targetUserId} за период {dateFrom:dd.MM.yyyy}-{dateTo:dd.MM.yyyy} не найдено.", cancellationToken: ctx.CancellationToken);
                return;
            }

            var acc = AccountProvider.Accounts.FirstOrDefault(a => a.TelegramUserId.ToString() == targetUserId);
            var displayName = acc?.UserName ?? "(Unknown)";

            var lines = userReports.Select(r => $"{displayName} {r.ReportDate:dd.MM.yyyy} {r.HoursWorked}");
            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, string.Join("\n", lines), cancellationToken: ctx.CancellationToken);
        }
        else
        {
            if (!_parser.TryParse(ctx.Argument, out var dateFrom, out var dateTo))
            {
                await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, "Неверный формат дат. Пример: /stats 01.02.2025-06.02.2025", cancellationToken: ctx.CancellationToken);
                return;
            }

            var filtered = DatabaseHelper.GetReportsByDateRange(dateFrom, dateTo);
            if (!filtered.Any())
            {
                await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, $"Отчётов за период {dateFrom:dd.MM.yyyy}-{dateTo:dd.MM.yyyy} не найдено.", cancellationToken: ctx.CancellationToken);
                return;
            }

            var grouped = filtered
                .GroupBy(r => r.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalHours = g.Sum(x => x.HoursWorked),
                    UserName = AccountProvider.GetUserNameById(Convert.ToInt64(g.Key))
                })
                .OrderBy(x => x.UserName)
                .ToList();

            string statsMsg = $"Статистика за {dateFrom:dd.MM.yyyy}-{dateTo:dd.MM.yyyy}:\n\n";
            foreach (var item in grouped)
            {
                statsMsg += $"{item.UserName} — {item.TotalHours} ч.\n";
            }

            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, statsMsg, cancellationToken: ctx.CancellationToken);
        }
    }
}