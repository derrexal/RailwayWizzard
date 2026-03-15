namespace RailwayWizzard.Rzd.DataEngine.Exceptions;


public class DontRunException : BusinessException
{
    public const string dontRunMessage = "В УКАЗАННУЮ ДАТУ ПОЕЗД НЕ ХОДИТ";

    private DontRunException(string? message)
        : base(message)
    {
    }

    public DontRunException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public static DontRunException New(int taskId)
    {
        var message =
            $"Task ID: {taskId} Сервис РЖД при запросе списка свободных мест вернул ошибку: {dontRunMessage}.";
        return new DontRunException(message);
    }
}