using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace TaskManager;

public sealed class NotificationScheduler : BackgroundService
{
    private readonly ITelegramBotClient _botClient;

    // Чтобы не спамить после наступления времени — запоминаем, кому за сегодняшний день уже отправляли.
    private readonly ConcurrentDictionary<long, DateOnly> _notifiedForDate = new();

    public NotificationScheduler(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Первый тик сразу, далее — раз в минуту
        var period = TimeSpan.FromMinutes(1);
        try
        {
            Log.I("Запуск фонового планировщика уведомлений.");

            // Немедленный запуск (без overlap — один поток BackgroundService)
            await TickAsync(stoppingToken);

            using var timer = new PeriodicTimer(period);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                Log.D("Новый тик планировщика уведомлений.");
                await TickAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Нормальное завершение по токену
        }
        catch (Exception ex)
        {
            Log.C(ex, "Фоновый планировщик уведомлений аварийно завершился.");
            // Опционально: можно повторно пробросить, если требуется падение хоста
        }
        finally
        {
            Log.I("Фоновый планировщик уведомлений остановлен.");
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        try
        {
            var utcNow = DateTime.UtcNow;

            foreach (var acc in AccountProvider.Accounts)
            {
                ct.ThrowIfCancellationRequested();

                if (acc.Role != Role.User)
                    continue;

                var userId = acc.TelegramUserId;
                var reportTime = acc.ReportDeadlineLocalTime;
                var tz = ResolveTimeZone(acc.TimeZoneId);

                var userNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
                var userToday = DateOnly.FromDateTime(userNow);
                var userLocalTime = userNow.TimeOfDay;

                // Пропускаем, если пользователь в отпуске (до указанной даты включительно)
                if (VacationRegistry.IsOnVacation(userId, userToday))
                {
                    _notifiedForDate.TryRemove(userId, out _);
                    continue;
                }

                // Пропускаем выходные (суббота и воскресенье)
                if (userNow.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    _notifiedForDate.TryRemove(userId, out _);
                    continue;
                }

                // Наступило ли локальное время пользователя?
                if (userLocalTime < reportTime)
                {
                    _notifiedForDate.TryRemove(userId, out _);
                    continue;
                }

                // Уже уведомляли сегодня?
                if (_notifiedForDate.TryGetValue(userId, out var lastDate) && lastDate == userToday)
                    continue;

                // Есть ли отчет за локальную "сегодня" пользователя?
                if (HasUserReportForDate(userId, userNow.Date))
                {
                    _notifiedForDate.TryRemove(userId, out _);
                    continue;
                }

                var text = "Напоминание: Вы не отправили дневной отчет за сегодня. Пожалуйста, отправьте его.";
                try
                {
                    await _botClient.SendTextMessageAsync(chatId: userId, text: text, cancellationToken: ct);
                    _notifiedForDate[userId] = userToday;
                    Log.I($"Отправлено напоминание пользователю {userId} за {userToday:yyyy-MM-dd}.");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception sendEx)
                {
                    Log.W($"Не удалось отправить напоминание пользователю {userId}: {sendEx.Message}");
                    Log.D($"StackTrace: {sendEx.StackTrace ?? "<нет стека>"}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.E(ex, "Ошибка в тикe планировщика уведомлений.");
        }
    }

    // Простейшая проверка наличия отчёта за дату (как и было)
    private static bool HasUserReportForDate(long userId, DateTime date)
    {
        try
        {
            var from = date.Date;
            var to = date.Date;
            var reports = DatabaseHelper.GetReportsByDateRange(from, to);
            return reports.Any(r => r.UserId == userId.ToString());
        }
        catch (Exception ex)
        {
            Log.W($"Не удалось проверить наличие отчёта пользователя {userId} за {date:yyyy-MM-dd}: {ex.Message}");
            Log.D($"StackTrace: {ex.StackTrace ?? "<нет стека>"}");
            return false;
        }
    }

    // Разрешение таймзоны. Поддерживает строки вида "UTC+04:00" / "UTC-03" / "UTC".
    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return TimeZoneInfo.CreateCustomTimeZone("UTC+04:00", TimeSpan.FromHours(4), "UTC+04:00", "UTC+04:00");

        if (TryParseUtcOffset(timeZoneId, out var tz))
            return tz;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception ex)
        {
            Log.W($"Не удалось определить таймзону '{timeZoneId}': {ex.Message}. Используем UTC+04:00.");
            Log.D($"StackTrace: {ex.StackTrace ?? "<нет стека>"}");
            // Фоллбэк на дефолт UTC+04:00
            return TimeZoneInfo.CreateCustomTimeZone("UTC+04:00", TimeSpan.FromHours(4), "UTC+04:00", "UTC+04:00");
        }
    }

    private static bool TryParseUtcOffset(string value, out TimeZoneInfo tz)
    {
        tz = TimeZoneInfo.Utc;
        var v = value.Trim();

        if (v.Equals("UTC", StringComparison.OrdinalIgnoreCase))
        {
            tz = TimeZoneInfo.Utc;
            return true;
        }

        if (v.StartsWith("UTC", StringComparison.OrdinalIgnoreCase))
            v = v.Substring(3).Trim();

        if (!v.StartsWith("+") && !v.StartsWith("-"))
            return false;

        var sign = v[0] == '-' ? -1 : 1;
        var body = v.Substring(1);

        int hours, minutes = 0;
        if (body.Contains(':'))
        {
            var parts = body.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out hours)) return false;
            if (!int.TryParse(parts[1], out minutes)) return false;
        }
        else
        {
            if (!int.TryParse(body, out hours)) return false;
        }

        if (hours is < 0 or > 14) return false;
        if (minutes is < 0 or > 59) return false;

        var offset = new TimeSpan(sign * hours, sign * minutes, 0);
        tz = TimeZoneInfo.CreateCustomTimeZone(
            $"UTC{(sign < 0 ? "-" : "+")}{hours:00}:{minutes:00}",
            offset,
            $"UTC{(sign < 0 ? "-" : "+")}{hours:00}:{minutes:00}",
            "UTC");
        return true;
    }
}
