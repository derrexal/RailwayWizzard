namespace RailwayWizzard.Common;

public static class Ensure
{
    // похоже на то, что тоже должно быть в StringExtensions
    public static void IsNotNullOrEmptyOrWhiteSpace(this string text)
    {
        // IsNullOrWhiteSpace проверяет и на Empty тоже
        if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)) 
            throw new ArgumentException($"This string is empty or white space.");
    }
}