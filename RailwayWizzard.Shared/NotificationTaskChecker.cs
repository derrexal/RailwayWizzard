﻿using System.Globalization;
using RailwayWizzard.Core;


namespace RailwayWizzard.Shared
{
    /// <summary>
    /// Класс, содержаший вспомогательные методы, имеет отношение к модели "NotificationTask:
    /// </summary>
    public class NotificationTaskChecker: IChecker
    {

        /// <summary>
        /// Проверяет задачу на актуальность
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool CheckActualNotificationTask(NotificationTask task)
        {
            DateTime itemDateFromDateTime = DateTime.ParseExact(
                        task.DateFrom.ToShortDateString() + " " + task.TimeFrom,
                        "MM/dd/yyyy HH:mm",
                        CultureInfo.InvariantCulture);

            if (itemDateFromDateTime < DateTime.Now)
                return false;
            return true;
        }
    }
}