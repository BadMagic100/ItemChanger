using ItemChanger.Logging;

namespace ItemChanger;

/// <summary>
/// Public hook point to have ItemChanger inject its logs to a logger of the caller's choosing.
/// </summary>
public static class LoggerProxy
{
    public static ILogger Logger { get; set; } = new NullLogger();

    internal static void LogFine(string? message)
    {
        Logger.LogFine(message);
    }

    internal static void LogInfo(string? message)
    {
        Logger.LogInfo(message);
    }

    internal static void LogWarn(string? message)
    {
        Logger.LogWarn(message);
    }

    internal static void LogError(string? message)
    {
        Logger.LogError(message);
    }
}
