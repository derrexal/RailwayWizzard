namespace RailwayWizzard.Common;

public static class Ensure
{
    public static void IsNotNullOrEmptyOrWhiteSpace(this string text)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)) 
            throw new ArgumentException($"This string is empty or white space.");
    }
}