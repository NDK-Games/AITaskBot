using System;
using System.IO;
using DotNetEnv;

namespace TaskManager;

public static class Environment
{
    private const string OpenAiKeyVariable = "OPENAI_API_KEY";
    private const string TelegramTokenVariable = "TG_BOT_TOKEN";

    static Environment()
    {
        LoadEnvironment();
        OpenAiApiKey = GetRequiredVariable(OpenAiKeyVariable);
        TelegramBotToken = GetRequiredVariable(TelegramTokenVariable);
    }

    public static string OpenAiApiKey { get; }

    public static string TelegramBotToken { get; }

    private static void LoadEnvironment()
    {
        var envFileName = ".env.release";
#if DEBUG
        envFileName = ".env.debug";
#endif
        var envPath = Path.Combine(AppContext.BaseDirectory, envFileName);
        if (File.Exists(envPath))
        {
            Log.I($"Загрузка переменных окружения из {envPath}.");
            Env.Load(envPath);
        }
        else
        {
            Log.W($"Файл {envPath} не найден, загружаем окружение из стандартного расположения.");
            Env.Load();
        }
    }

    private static string GetRequiredVariable(string variableName)
    {
        var value = global::System.Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Переменная окружения '{variableName}' не установлена.");
        }

        return value;
    }
}
