using TaskManager;
using Telegram.Bot;

public class ListCommand : ICommand
{
    public bool CanHandle(string command) => string.Equals(command, "/list", StringComparison.OrdinalIgnoreCase);

    public async Task ExecuteAsync(CommandContext ctx)
    {
        if (ctx.Account == null || ctx.Account.Role == Role.User)
        {
            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, "У вас недостаточно прав для этой команды.", cancellationToken: ctx.CancellationToken);
            return;
        }

        var all = DatabaseHelper.GetAllAccounts();
        if (all.Count == 0)
        {
            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, "Список аккаунтов пуст.", cancellationToken: ctx.CancellationToken);
            return;
        }

        var lines = await Task.WhenAll(all.Select(async a =>
        {
            var id = a.TelegramUserId;
            var userName = a.UserName;
            var role = a.Role;
            return $"Name={userName}, ID={id}, Role={role}";
        }));

        string listText = "Список аккаунтов:\n" + string.Join("\n", lines);
        await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, listText, cancellationToken: ctx.CancellationToken);
    }
}