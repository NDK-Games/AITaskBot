using TaskManager;
using Telegram.Bot;

public class AddRoleCommand : ICommand
{
    private static readonly HashSet<string> Names = new(StringComparer.OrdinalIgnoreCase)
    {
        "/add_admin", "/add_moderator", "/add_user"
    };

    public bool CanHandle(string command) => Names.Contains(command);

    public async Task ExecuteAsync(CommandContext ctx)
    {
        if (ctx.Account?.Role != Role.Admin)
        {
            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, "Эта команда доступна только администратору.", cancellationToken: ctx.CancellationToken);
            return;
        }

        if (string.IsNullOrEmpty(ctx.Argument))
        {
            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, $"Укажите: {ctx.Command} [TelegramID]", cancellationToken: ctx.CancellationToken);
            return;
        }

        Role newRole = ctx.Command.Equals("/add_admin", StringComparison.OrdinalIgnoreCase) ? Role.Admin
            : ctx.Command.Equals("/add_moderator", StringComparison.OrdinalIgnoreCase) ? Role.Moderator
            : Role.User;

        DatabaseHelper.AddOrUpdateAccount(ctx.Argument, newRole);
        await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, $"Теперь пользователь {ctx.Argument} имеет роль {newRole}.", cancellationToken: ctx.CancellationToken);
    }
}