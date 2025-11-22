namespace ItemChanger.Logging;

public interface ILogger
{
    /// <summary>
    /// Logs verbose diagnostic information.
    /// </summary>
    void LogFine(string? message);

    /// <summary>
    /// Logs high-level informational messages.
    /// </summary>
    void LogInfo(string? message);

    /// <summary>
    /// Logs warning messages indicating potential issues.
    /// </summary>
    void LogWarn(string? message);

    /// <summary>
    /// Logs errors that require attention.
    /// </summary>
    void LogError(string? message);
}
