namespace ItemChanger.Logging;

/// <summary>
/// Abstraction for logging messages at various severity levels.
/// </summary>
public interface ILogger
{
    public void LogFine(string? message);

    public void LogInfo(string? message);

    public void LogWarn(string? message);

    public void LogError(string? message);
}
