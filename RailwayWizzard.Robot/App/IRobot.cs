using RailwayWizzard.Core;


namespace RailwayWizzard.Robot.App
{
    /// <summary>
    /// Возвращает информацию от сервисов РЖД
    /// </summary>
    public interface IRobot
    {
        /// <summary>
        /// Получение информации о свободных местах в запрашиваемом рейсе
        /// </summary>
        /// <param name="inputNotificationTask">Запрашиваемый рейс</param>
        /// <returns>Информации о свободных местах</returns>
        public Task<string> GetFreeSeatsOnTheTrain(NotificationTask inputNotificationTask);

        /// <summary>
        /// Возвращает ссылку на сайт РЖД для покупки билета
        /// </summary>
        /// <returns>Ссылка</returns>
        public Task<string?> GetLinkToBuyTicket(NotificationTask notificationTask);

        /// <summary>
        /// Если свободные места были, а сейчас их уже нет.
        /// </summary>
        /// <param name="notificationTaskText"></param>
        /// <returns></returns>
        public string GetMessageSeatsIsEmpty(string notificationTaskText);

        /// <summary>
        /// Если свободных мест не было, а сейчас они появились
        /// Или если изменилось количество свободных мест
        /// </summary>
        /// <param name="notificationTaskText"></param>
        /// <returns></returns>
        public Task<string> GetMessageSeatsIsComplete(NotificationTask notificationTask, string resultFreeSeats);

    }
}