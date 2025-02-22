namespace RailwayWizzard.Telegram.ApiClient.Exceptions;

/// <summary>
/// 409 Http Error. 
/// </summary>
public class ConflictHttpException : HttpRequestException
{
    public ConflictHttpException(string message) : base(message)
    {
    }
}
