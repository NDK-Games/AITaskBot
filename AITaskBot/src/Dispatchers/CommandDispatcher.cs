using Telegram.Bot;
using Telegram.Bot.Types;
using TaskManager;

public record CommandContext(
    ITelegramBotClient Bot,
    Message Message,
    CancellationToken CancellationToken,
    Account? Account,
    string Command,
    string Argument
);

public interface ICommand
{
    bool CanHandle(string command);
    Task ExecuteAsync(CommandContext ctx);
}

public class CommandDispatcher
{
    private readonly IReadOnlyList<ICommand> _commands;

    public CommandDispatcher(IEnumerable<ICommand> commands)
    {
        _commands = commands.ToList();
    }

    public async Task<bool> DispatchAsync(CommandContext ctx)
    {
        var cmd = _commands.FirstOrDefault(c => c.CanHandle(ctx.Command));
        if (cmd == null) return false;
        await cmd.ExecuteAsync(ctx);
        return true;
    }
}