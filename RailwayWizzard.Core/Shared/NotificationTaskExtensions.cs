using RailwayWizzard.Shared;
using System.Globalization;

namespace RailwayWizzard.Core.Shared
{
    /// <summary>
    /// Содержит методы расширения для <see cref="NotificationTask"/>
    /// </summary>
    public static class NotificationTaskExtensions
    {
        /// <summary>
        /// Проверяет задачу на актуальность по московскому времени.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static bool IsActuality(this NotificationTask task)
        {
            DateTime notificationTaskDateTime = DateTime.ParseExact(
                task.DateFrom.ToShortDateString() + " " + task.TimeFrom,
                "MM/dd/yyyy HH:mm",
                CultureInfo.InvariantCulture);

            var moscowDateTime = Common.MoscowNow;

            var notificationTaskIsActual = notificationTaskDateTime > moscowDateTime;
            if (notificationTaskIsActual)
                return true;
            return false;
        }
    }
}