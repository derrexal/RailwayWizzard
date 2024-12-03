using RailwayWizzard.Shared;

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
        public static bool IsActuality(this NotificationTask task) => task.DepartureDateTime > Common.MoscowNow;
    }
}