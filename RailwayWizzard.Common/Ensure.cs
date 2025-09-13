namespace RailwayWizzard.Common;

/// <summary>
/// Вспомогательный класс для проверки объектов.
/// </summary>
public static class Ensure
{
    private const string DefaultParameterName = "Variable";

    private static string GenerateNullErrorMessage(string? parameterName) =>
        $"{parameterName ?? DefaultParameterName} cannot be null.";

    public static T NotNull<T>(T? value, string? parameterName = null) where T : class => 
        value ?? throw new ArgumentNullException(GenerateNullErrorMessage(parameterName));
    
    public static T NotNull<T>(T? value, string? parameterName = null) where T : struct => 
        value ?? throw new ArgumentNullException(GenerateNullErrorMessage(parameterName));
    
    public static void IsNotNullOrEmptyOrWhiteSpace(this string text)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)) 
            throw new ArgumentException($"This string is empty or white space.");
    }
}