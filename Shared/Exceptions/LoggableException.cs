namespace ChrisUsher.Core.Shared.Exceptions;

public class LoggableException : Exception
{
    private readonly bool _shouldLog;

    public LoggableException(string message, bool shouldLog = false) : base(message)
    {
        _shouldLog = shouldLog;
    }

    public bool ShouldLog() => _shouldLog;
}
