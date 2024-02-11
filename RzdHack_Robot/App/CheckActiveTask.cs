using RzdHack_Robot.Core;

namespace RzdHack_Robot.App
{
    public static class CheckActiveTask
    {
        /// <summary>
        /// Смотрим в базу на актуальные таски и запускаем их
        /// Пока что не устанавливал ограничение
        /// </summary>
        public static void Start()
        {
            var notificationTasksList = new NotificationTaskApp().GetActive();
            //перебираем все таски из таблицы и создаем на каждый задачу
            foreach (var notificationTask in notificationTasksList)
            {   //пока не хочется убирать этот класс из логики работающего приложения приходится маппить в эту сущность
                var railwayInput = new RailwayInput
                {
                    DepartureStation = notificationTask.DepartureStation,
                    ArrivalStation = notificationTask.ArrivalStation,
                    DateFrom = notificationTask.DateFrom,
                    CurrentTime = notificationTask.TimeFrom,
                    UserID = notificationTask.UserId
                };
                //заккоментировал чтобы ошибка не трезвнила
                //var t = new Thread(() => new StepsUsingDriver().Notification(railwayInput));
                //                t.Start();
            }
        }
    }
}