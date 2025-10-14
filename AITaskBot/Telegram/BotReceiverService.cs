using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;

public class BotReceiverService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly IMessageUpdateRouter _router;

    public BotReceiverService(ITelegramBotClient bot, IMessageUpdateRouter router)
    {
        _bot = bot;
        _router = router;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        _bot.StartReceiving(
            _router.HandleUpdateAsync,
            _router.HandleErrorAsync,
            receiverOptions,
            stoppingToken
        );
        return Task.CompletedTask;
    }
}
