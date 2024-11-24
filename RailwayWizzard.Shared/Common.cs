using System.Globalization;

namespace RailwayWizzard.Shared
{
    /// <summary>
    /// Вспомогательный класс
    /// </summary>
    public static class Common
    {
        private static CultureInfo _russianCultureInfo = new("ru-RU");
        public static CultureInfo RussianCultureInfo
        {
            get
            {
                return _russianCultureInfo;
            }
        }

        /// <summary>
        /// Возвращает текущую дату и время по Москве
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime MoscowNow
        {
            get
            {
                // Find the time zone for Moscow
                TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

                // Convert the UTC time to Moscow time
                DateTime moscowDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone);

                // Return Moscow DateTime
                return moscowDateTime;

            }
        }

        /// <summary>
        /// Возвращает результат проверки текущего времени на попадание в технические работы у РЖД (03:30-04:00).
        /// </summary>
        /// <returns>Результат проверки.</returns>
        public static bool IsDownTimeRzd()
        {
            var startDownTime = new TimeOnly(03, 30);
            var endDownTime = new TimeOnly(04, 00);

            var todayTime = TimeOnly.FromDateTime(MoscowNow);

            var IsDownTime = startDownTime < todayTime && todayTime < endDownTime;

            return IsDownTime;
        }
    }
}