using System;

namespace TaskManager;

/// <summary>
/// Универсальный облегчённый логгер для консоли.
/// - Поддерживает уровни: Trace, Debug, Information, Warning, Error, Critical.
/// - В отладочных сборках минимальный уровень по умолчанию — Trace; в релизе — Information.
/// - Error/Critical отправляются в стандартный поток ошибок (stderr), остальные — в stdout.
/// - Можно динамически менять порог детальности через <see cref="MinimumLevel"/>.
/// Рекомендуемое применение уровней:
/// - Trace — самый подробный уровень: пошаговые трассировки, диагностика производительности, вход/выход из методов.
/// - Debug — отладочная информация для разработчиков: ключевые параметры, ветвления логики.
/// - Information — нормальный рабочий ход приложения: старты/остановки, завершение задач, важные бизнес-события.
/// - Warning — потенциально проблемные ситуации, которые пока не приводят к ошибкам (ретраи, деградация).
/// - Error — ошибки, из-за которых операция не выполнена, но приложение продолжает работать (обработанные исключения).
/// - Critical — фатальные ошибки, требующие немедленного внимания (невозможен дальнейший прогресс, потеря данных).
/// </summary>
public static class Log
{
    /// <summary>
    /// Уровень важности сообщения лога.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Самый детальный уровень. Пошаговая трассировка, подробности исполнения, шумная диагностика.
        /// Используйте для поиска сложных багов и измерений производительности на локальной среде.
        /// </summary>
        Trace = 0,
        /// <summary>
        /// Отладочная информация для разработчиков. Важные внутренние детали: параметры, ветви условий, результаты вызовов.
        /// Обычно выключается в продакшене.
        /// </summary>
        Debug = 1,
        /// <summary>
        /// Информационные сообщения о нормальной работе приложения: запуск/остановка сервисов, ключевые действия пользователя,
        /// завершение задач, контрольные точки бизнес-процессов.
        /// </summary>
        Information = 2,
        /// <summary>
        /// Предупреждения о нестандартных или потенциально проблемных ситуациях: ретраи, таймауты, деградация функциональности,
        /// автоматическое восстановление. Не требует срочного вмешательства, но требует внимания.
        /// </summary>
        Warning = 3,
        /// <summary>
        /// Ошибки, из-за которых операция не выполнена (исключение обработано), но приложение продолжает работу.
        /// Требует расследования и исправления.
        /// </summary>
        Error = 4,
        /// <summary>
        /// Критические ошибки, ставящие под угрозу работоспособность, безопасность или данные.
        /// Обычно сопровождаются алертами и немедленным реагированием.
        /// </summary>
        Critical = 5,
        /// <summary>
        /// Отключение логирования. Сообщения с этим уровнем никогда не выводятся.
        /// </summary>
        None = 6
    }

#if DEBUG
    private static readonly bool IsDebug = true;
#else
    private static readonly bool IsDebug = false;
#endif

    /// <summary>
    /// Минимальный уровень сообщений, которые будут выводиться логгером.
    /// Можно изменить в рантайме, например: <c>Log.MinimumLevel = Log.LogLevel.Warning;</c>
    /// По умолчанию: Trace в DEBUG, Information в RELEASE.
    /// </summary>
    public static LogLevel MinimumLevel { get; set; } = IsDebug ? LogLevel.Trace : LogLevel.Information;

    // Базовый метод логирования
    private static void Write(LogLevel level, string message)
    {
        if (level < MinimumLevel || level == LogLevel.None) return;

        var prefix = level switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRITICAL",
            _ => "LOG"
        };

        var line = $"[{DateTime.UtcNow:G}] [{prefix}] {message}";

        // Пишем напрямую, чтобы избежать рекурсии
        if (level >= LogLevel.Error)
            Console.Error.WriteLine(line);
        else
            Console.Out.WriteLine(line);
    }

    /// <summary>
    /// Trace: наиболее подробные трассировки и диагностические сообщения.
    /// Используйте для пошагового анализа выполнения и сложных отладочных сценариев.
    /// </summary>
    public static void T(string message) => Write(LogLevel.Trace, message);

    /// <summary>
    /// Debug: отладочные сообщения, полезные разработчикам.
    /// В продакшене обычно скрыты повышением <see cref="MinimumLevel"/>.
    /// </summary>
    public static void D(string message) => Write(LogLevel.Debug, message);

    /// <summary>
    /// Information: ключевые события нормального выполнения приложения.
    /// Рекомендуется оставлять включённым в продакшене.
    /// </summary>
    public static void I(string message) => Write(LogLevel.Information, message);

    /// <summary>
    /// Warning: потенциальные проблемы, нестандартные ситуации, автоматические восстановления.
    /// </summary>
    public static void W(string message) => Write(LogLevel.Warning, message);

    /// <summary>
    /// Error: ошибки операций (исключение обработано), приложение продолжает работу.
    /// Сообщения пишутся в стандартный поток ошибок (stderr).
    /// </summary>
    public static void E(string message) => Write(LogLevel.Error, message);

    /// <summary>
    /// Critical: фатальные ошибки, требующие немедленного внимания (алерты).
    /// Сообщения пишутся в стандартный поток ошибок (stderr).
    /// </summary>
    public static void C(string message) => Write(LogLevel.Critical, message);

    /// <summary>
    /// Упрощённая запись ошибки вместе с исключением.
    /// </summary>
    public static void E(Exception ex, string? message = null)
        => Write(LogLevel.Error, FormatException(message, ex));

    /// <summary>
    /// Упрощённая запись критической ошибки вместе с исключением.
    /// </summary>
    public static void C(Exception ex, string? message = null)
        => Write(LogLevel.Critical, FormatException(message, ex));

    private static string FormatException(string? message, Exception ex)
    {
        var header = string.IsNullOrWhiteSpace(message) ? "Exception" : message.Trim();
        return $"{header}: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
    }
}
