using TaskManager;
using Telegram.Bot;

public class RemoveUserCommand : ICommand
{
    public bool CanHandle(string command) => string.Equals(command, "/remove_user", StringComparison.OrdinalIgnoreCase);

    public async Task ExecuteAsync(CommandContext ctx)
    {
        if (ctx.Account?.Role != Role.Admin)
        {
            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, "Эта команда доступна только администратору.", cancellationToken: ctx.CancellationToken);
            return;
        }

        if (string.IsNullOrEmpty(ctx.Argument))
        {
            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, "Укажите: /remove_user [TelegramID]", cancellationToken: ctx.CancellationToken);
            return;
        }

        bool removed = DatabaseHelper.RemoveAccount(ctx.Argument);
        string msg = removed
            ? $"Пользователь {ctx.Argument} удалён из базы."
            : $"Пользователь {ctx.Argument} не найден в базе.";
        await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, msg, cancellationToken: ctx.CancellationToken);
    }
}