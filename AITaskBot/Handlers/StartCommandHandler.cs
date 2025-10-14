using Telegram.Bot;

public class StartCommand : ICommand
{
    public bool CanHandle(string command) => string.Equals(command, "/start", StringComparison.OrdinalIgnoreCase);

    public async Task ExecuteAsync(CommandContext ctx)
    {
        string welcomeMessage =
            "👋 Привет! Я бот для приема отчетов. Пожалуйста, отправьте отчет в следующем формате:\n\n" +
            "📋 <b>Формат отчета:</b>\n\n" +
            "Имя: [Ваше имя]\n" +
            "Дата: [ДД.ММ.ГГГГ]\n" +
            "Часов: [Количество отработанных часов]\n" +
            "Сделано:\n- [Описание выполненной работы]\n" +
            "Проблемы:\n- [Если есть, перечислите]\n" +
            "Планируется:\n- [Планы на завтра/дальнейшую работу]";
        await ctx.Bot.SendTextMessageAsync(ctx.Message.Chat.Id, welcomeMessage, cancellationToken: ctx.CancellationToken);
    }
}