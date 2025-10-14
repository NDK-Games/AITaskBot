using System;

namespace TaskManager;

public static class Log
{
    public static void I(string message) => Write("INFO", message);

    public static void W(string message) => Write("WARN", message);

    public static void E(string message) => Write("ERROR", message);

    private static void Write(string level, string message)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] {level}: {message}");
    }
}
