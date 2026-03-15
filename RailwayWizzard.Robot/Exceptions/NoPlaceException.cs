namespace RailwayWizzard.Rzd.DataEngine.Exceptions;


public class NoPlaceException : BusinessException
{
    public const string noPlaceMessage = "МЕСТ НЕТ";

    private NoPlaceException(string? message)
        : base(message)
    {
    }

    public NoPlaceException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public static NoPlaceException New(int taskId)
    {
        var message =
            $"Task ID: {taskId} Сервис РЖД при запросе списка свободных мест вернул ошибку: {noPlaceMessage}.";
        return new NoPlaceException(message);
    }
}