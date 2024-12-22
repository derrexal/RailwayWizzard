using System.Globalization;

namespace RailwayWizzard.Shared
{
    /// <summary>
    /// Вспомогательный класс.
    /// </summary>
    public static class Common
    {
        public static CultureInfo RussianCultureInfo { get; } = new("ru-RU");

        private static readonly TimeZoneInfo MoscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
        
        /// <summary>
        /// Возвращает текущую дату и время по Москве.
        /// </summary>
        /// <returns></returns>
        public static DateTime MoscowNow =>
            // Convert the UTC time to Moscow time
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MoscowTimeZone);

        /// <summary>
        /// Возвращает результат проверки текущего времени на попадание в технические работы у РЖД (03:30-04:00).
        /// </summary>
        /// <returns>Результат проверки.</returns>
        public static bool IsDownTimeRzd()
        {
            var startDownTime = new TimeOnly(03, 30);
            var endDownTime = new TimeOnly(04, 00);

            var todayTime = TimeOnly.FromDateTime(MoscowNow);

            var isDownTime = startDownTime < todayTime && todayTime < endDownTime;

            return isDownTime;
        }
    }
}