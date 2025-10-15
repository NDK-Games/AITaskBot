namespace TaskManager;

public class AccountProvider
{
    private const string DefaultTimeZoneId = "UTC+04:00";

    // Храним сразу список аккаунтов с заполненными полями Account
    public static readonly List<Account> Accounts = new List<Account>
    {
        new Account { UserName = "Dmitriy Nikitin", TelegramUserId = 117101673,  Role = Role.Admin, TimeZoneId = "UTC+04:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Elizaveta Nikitinа", TelegramUserId = 852505566,  Role = Role.Moderator, TimeZoneId = "UTC+04:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Elena Nikitinа", TelegramUserId = 295868165,  Role = Role.Moderator, TimeZoneId = "UTC+04:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Arazmedov German", TelegramUserId = 192314463,  Role = Role.User,  TimeZoneId = "UTC+04:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Alina Boburova", TelegramUserId = 1674848860, Role = Role.User,  TimeZoneId = "UTC+04:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Slava Elaurkin", TelegramUserId = 896741685,  Role = Role.User,  TimeZoneId = "UTC+04:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Egor Viryaskin", TelegramUserId = 194182828,  Role = Role.User,  TimeZoneId = "UTC+04:00", ReportDeadlineLocalTime = TimeSpan.FromHours(23) },
        new Account { UserName = "Fedor Tarakanov", TelegramUserId = 7474260465, Role = Role.User,  TimeZoneId = "UTC+03:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Egor Ponomarenko", TelegramUserId = 964038938,  Role = Role.User,  TimeZoneId = "UTC+03:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Andrey Inozemcev", TelegramUserId = 302019992,  Role = Role.User,  TimeZoneId = "UTC+07:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Kirill Plescov", TelegramUserId = 311148496,  Role = Role.User,  TimeZoneId = "UTC+07:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Anastasia Mavrina", TelegramUserId = 1630341281, Role = Role.User,  TimeZoneId = "UTC+04:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Vladimir Blem", TelegramUserId = 1937336138, Role = Role.User,  TimeZoneId = "UTC+04:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Kirill Erin", TelegramUserId = 5674052214, Role = Role.User,  TimeZoneId = "UTC+03:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) },
        new Account { UserName = "Pavel Semenov", TelegramUserId = 927754996, Role = Role.User,  TimeZoneId = "UTC+04:00", ReportDeadlineLocalTime = TimeSpan.FromHours(20) }
    };
    
    public static string GetUserNameById(long userId)
    {
        return Accounts.FirstOrDefault(acc => acc.TelegramUserId == userId)?.UserName ?? "(Unknown)";
    }
}