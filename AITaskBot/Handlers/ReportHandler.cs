using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TaskManager;

public interface IReportHandler
{
    Task ProcessAsync(ITelegramBotClient bot, Message message, Account? account, CancellationToken ct);
}

public class ReportHandler : IReportHandler
{
    public async Task ProcessAsync(ITelegramBotClient bot, Message message, Account? account, CancellationToken ct)
    {
        if (account == null)
        {
            await bot.SendTextMessageAsync(message.Chat.Id, "У вас нет роли в системе. Обратитесь к администратору.", cancellationToken: ct);
            return;
        }

        var userId = message.From!.Id;
        var lastReport = DatabaseHelper.GetLastReportByUser(userId.ToString());
        if (lastReport != null)
        {
            var timeSinceLastReport = DateTime.Now - lastReport.ReceivedAt;
            if (timeSinceLastReport.TotalHours < 23)
            {
                var timeLeft = 23 - timeSinceLastReport.TotalHours;
                await bot.SendTextMessageAsync(message.Chat.Id, $"Вы можете отправлять отчёт не чаще 1 раза в 24 часа. Попробуйте снова через {timeLeft:F0} часов.", cancellationToken: ct);
                return;
            }
        }

        string reportText = message.Text!.Trim();
        var requiredLines = new (string Required, string ErrorExplanation)[]
        {
            ("Имя:",        "В вашем отчете нет или неверна строка \"Имя:\""),
            ("Дата:",       "В вашем отчете нет или неверна строка \"Дата:\""),
            ("Часов:",      "В вашем отчете нет или неверна строка \"Часов:\""),
            ("Сделано:",    "В вашем отчете нет или неверна строка \"Сделано:\""),
            ("Проблемы:",   "В вашем отчете нет или неверна строка \"Проблемы:\". Если нет проблем, напишите \"Нет\""),
            ("Планируется:","В вашем отчете нет или неверна строка \"Планируется:\". Если нет планов, напишите \"Нет\"")
        };

        foreach (var (required, explanation) in requiredLines)
        {
            if (!Regex.IsMatch(reportText, @$"(?im)^{Regex.Escape(required)}\s*.*$"))
            {
                string errorMsg =
                    "Неверный формат отчёта.\n\n" +
                    "Вы отправили отчет в неправильном формате.\n\n" +
                    "📋 <b>Формат отчета:</b>\n\n" +
                    "Имя: [Ваше имя]\n" +
                    "Дата: [ДД.ММ.ГГГГ]\n" +
                    "Часов: [Количество отработанных часов]\n" +
                    "Сделано:\n- [Описание выполненной работы]\n" +
                    "Проблемы:\n- [Если есть, перечислите]\n" +
                    "Планируется:\n- [Планы на завтра/дальнейшую работу]\n\n" +
                    explanation;

                await bot.SendTextMessageAsync(message.Chat.Id, errorMsg, cancellationToken: ct);
                return;
            }
        }

        var dateMatch = Regex.Match(reportText, @"(?im)^Дата:\s*(?<date>\d{2}\.\d{2}\.\d{4})");
        if (!dateMatch.Success ||
            !DateTime.TryParseExact(dateMatch.Groups["date"].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            await bot.SendTextMessageAsync(message.Chat.Id, "Неверный формат даты. Используйте ДД.ММ.ГГГГ (например, 06.02.2025).", cancellationToken: ct);
            return;
        }

        var hoursMatch = Regex.Match(reportText, @"(?im)^Часов:\s*(?<hours>\d+)");
        if (!hoursMatch.Success || !int.TryParse(hoursMatch.Groups["hours"].Value, out int hoursWorked))
        {
            await bot.SendTextMessageAsync(message.Chat.Id, "Невозможно определить количество часов (нужно целое число).", cancellationToken: ct);
            return;
        }

        var userName = AccountProvider.GetUserNameById(userId);

        var newReport = new DailyReport
        {
            UserId = userId.ToString(),
            UserName = userName,
            ReportDate = parsedDate,
            HoursWorked = hoursWorked,
            FullReportText = reportText,
            ReceivedAt = DateTime.Now
        };
        DatabaseHelper.InsertReport(newReport);

        await bot.SendTextMessageAsync(message.Chat.Id, "✅ Отчёт получен! Спасибо.", cancellationToken: ct);

        var allAdmins = DatabaseHelper.GetAllAdmins();
        if (allAdmins.Count > 0)
        {
            string sanitized = WebUtility.HtmlEncode(reportText);
            string msgToAdmins = $"📌 <b>Новый отчёт от {userName}:</b>\n\n{sanitized}";

            foreach (var adminAcc in allAdmins)
            {
                await bot.SendTextMessageAsync(adminAcc.TelegramUserId, msgToAdmins, parseMode: ParseMode.Html, cancellationToken: ct);
            }
        }
    }
}
