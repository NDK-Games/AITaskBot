using TaskManager;
using Telegram.Bot;

public class HelpCommand : ICommand
{
    private readonly HelpTextProvider _provider;

    public HelpCommand(HelpTextProvider provider) => _provider = provider;

    public bool CanHandle(string command) => string.Equals(command, "/help", StringComparison.OrdinalIgnoreCase);

    public async Task ExecuteAsync(CommandContext ctx)
    {
        if (ctx.Account == null)
        {
            await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, "У вас нет роли в системе. Обратитесь к администратору.", cancellationToken: ctx.CancellationToken);
            return;
        }

        string helpText = _provider.GetText(ctx.Account.Role);
        await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, helpText, cancellationToken: ctx.CancellationToken);
    }
}