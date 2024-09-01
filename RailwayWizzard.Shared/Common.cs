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
    }
}