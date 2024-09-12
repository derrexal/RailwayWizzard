namespace RailwayWizzard.Shared
{
    /// <summary>
    /// Вспомогательный класс
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Возвращает текущую дату и время по Москве
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetMoscowDateTime
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
        /// Возвращает результат проверки текущего времени на попадание в технические работы у РЖД (03:20-04:10).
        /// </summary>
        /// <returns>Результат проверки.</returns>
        public static bool IsDownTimeRzd()
        {
            var startDownTime = new TimeOnly(03, 20);
            var endDownTime = new TimeOnly(04, 10);

            var todayTime = TimeOnly.FromDateTime(GetMoscowDateTime);

            var IsDownTime = startDownTime < todayTime && todayTime < endDownTime;
            
            //DEBUG
            Console.WriteLine($"DEBUG DownTimeCheck todayTime:{todayTime} IsDownTime{IsDownTime}");
            
            return IsDownTime;
        }
    }
}