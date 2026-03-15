namespace RailwayWizzard.Rzd.DataEngine.Exceptions;


public class UnknownResponseException : BusinessException
{
    private UnknownResponseException(string? message)
        : base(message)
    {
    }

    public UnknownResponseException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public static UnknownResponseException New(int taskId, string response)
    {
        var message = $"Task ID: {taskId} Сервис РЖД при запросе списка свободных мест вернул не стандартный ответ. " +
            $"Ответ:{response}";
        return new UnknownResponseException(message);
    }
}