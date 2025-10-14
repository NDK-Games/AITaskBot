using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManager;
using Telegram.Bot;

class Program
{
    static async Task Main()
    {
        // Инициализация БД и первичное наполнение ролей
        DatabaseHelper.InitializeDatabase();
        foreach (var acc in AccountProvider.Accounts)
        {
            DatabaseHelper.AddOrUpdateAccount(acc.TelegramUserId.ToString(), acc.Role);
        }

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Бот-клиент и фоновые сервисы
                services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(TelegramAPI.Token));
                services.AddHostedService<NotificationScheduler>();
                services.AddHostedService<BotReceiverService>();

                // Обработчики и инфраструктура команд
                services.AddSingleton<IMessageUpdateRouter, MessageUpdateHandler>();
                services.AddSingleton<IReportHandler, ReportHandler>();
                services.AddSingleton<ICommand, StartCommand>();
                services.AddSingleton<ICommand, HelpCommand>();
                services.AddSingleton<ICommand, AddRoleCommand>();
                services.AddSingleton<ICommand, RemoveUserCommand>();
                services.AddSingleton<ICommand, ListCommand>();
                services.AddSingleton<ICommand, StatsCommand>();
                services.AddSingleton<ICommand, VacationCommand>();
                services.AddSingleton<CommandDispatcher>();

                // Хелперы
                services.AddSingleton<HelpTextProvider>();
                services.AddSingleton<DateRangeParser>();
            })
            .Build();

        await host.RunAsync();
    }
}
