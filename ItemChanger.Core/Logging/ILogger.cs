namespace ItemChanger.Logging;

public interface ILogger
{
    public void LogFine(string? message);

    public void LogInfo(string? message);

    public void LogWarn(string? message);

    public void LogError(string? message);
}
