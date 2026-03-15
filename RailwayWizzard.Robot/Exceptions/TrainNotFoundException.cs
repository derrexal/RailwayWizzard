namespace RailwayWizzard.Rzd.DataEngine.Exceptions;


public class TrainNotFoundException : BusinessException
{
    public const string noTrainsMessage = "В запрашиваемую дату поездов нет";

    private TrainNotFoundException(string? message)
        : base(message)
    {
    }

    public TrainNotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public static TrainNotFoundException New(int taskId)
    {
        var message = $"Task ID: {taskId} Сервис РЖД при запросе списка свободных мест вернул ошибку: {noTrainsMessage}.";
        return new TrainNotFoundException(message);
    }
}