public class Account
{
    // Primary Key в базе
    public int Id { get; set; }

    // Id пользователя в Telegram
    public long TelegramUserId { get; set; }  
    
    // Имя пользователя, хранимое в аккаунте (логическое отображаемое имя)
    public string? UserName { get; set; }
    
    // Роль
    public Role Role { get; set; }   
    
    // Часовой пояс пользователя. По умолчанию UTC+04:00
    public string TimeZoneId { get; set; } = "UTC+04:00";
    
    // Время (локальное для пользователя), к которому он должен отправить дневной отчет.
    // Используется также для времени отправки уведомления (если отчет не отправлен).
    public TimeSpan ReportDeadlineLocalTime { get; set; } = TimeSpan.FromHours(20);
}

public enum Role
{
    Admin,
    Moderator,
    User
}