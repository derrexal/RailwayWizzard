namespace RailwayWizzard.Rzd.DataEngine.Exceptions;

public class BusinessException : Exception
{
    // TODO: Добавить LogLevel?
    public BusinessException(string? message)
        : base(message)
    {
    }

    public BusinessException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}