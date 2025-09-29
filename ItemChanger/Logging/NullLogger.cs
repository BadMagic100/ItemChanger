namespace ItemChanger.Logging;

public class NullLogger : ILogger
{
    public static NullLogger Instance { get; } = new();

    public void LogFine(string? message)
    {
    }
    public void LogInfo(string? message)
    {
    }

    public void LogError(string? message)
    {
    }

    public void LogWarn(string? message)
    {
    }
}
