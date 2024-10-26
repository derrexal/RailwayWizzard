using RailwayWizzard.Core;


namespace RailwayWizzard.Robot.App
{
    /// <summary>
    /// Возвращает информацию от сервисов РЖД.
    /// </summary>
    public interface IRobot
    {
        /// <summary>
        /// Возвращает информацию о свободных местах в запрашиваемом рейсе.
        /// </summary>
        /// <param name="inputNotificationTask">Запрашиваемый рейс.</param>
        /// <returns>Информации о свободных местах.</returns>
        public Task<string> GetFreeSeatsOnTheTrain(NotificationTask inputNotificationTask);

        /// <summary>
        /// Возвращает ссылку на сайт РЖД для покупки билета.
        /// </summary>
        /// <returns>Ссылка для покупки билета.</returns>
        public Task<string?> GetLinkToBuyTicket(NotificationTask notificationTask);

        /// <summary>
        /// Возвращает сообщение пользователю на случай если свободные места были, а сейчас их уже нет.
        /// </summary>
        /// <param name="notificationTask"></param>
        /// <returns>Сообщение.</returns>
        public string GetMessageSeatsIsEmpty(NotificationTask notificationTask);

        /// <summary>
        /// Возвращает сообщение пользователю на случай если свободных мест не было, а сейчас они появились.
        /// Или если изменилось количество свободных мест.
        /// </summary>
        /// <param name="notificationTask"></param>
        /// <param name="resultFreeSeats"></param>
        /// <returns>Сообщение.</returns>
        public Task<string> GetMessageSeatsIsComplete(NotificationTask notificationTask, string resultFreeSeats);

    }
}