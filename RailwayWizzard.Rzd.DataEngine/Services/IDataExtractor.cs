using RailwayWizzard.Core.NotificationTask;
using RailwayWizzard.Rzd.ApiClient.Services.GetTrainInformationService.Models;

namespace RailwayWizzard.Rzd.DataEngine.Services
{
    /// <summary>
    /// Извлекает информацию от сервисов РЖД.
    /// </summary>
    public interface IDataExtractor
    {
        /// <summary>
        /// Извлекает информацию о свободных местах по запрашиваемому рейсу.
        /// </summary>
        /// <param name="task">Запрашиваемый рейс.</param>
        /// <returns>Информации о свободных местах.</returns>
        public Task<string> FindFreeSeatsAsync(NotificationTask task);

        /// <summary>
        /// Извлекает список доступного для бронирования времени.
        /// </summary>
        /// <param name="request">Модель запрашиваемого рейса.</param>
        /// <returns>Список доступного для бронирования времени.</returns>
        public Task<IReadOnlyCollection<string>> GetAvailableTimesAsync(GetTrainInformationRequest request);

        /// <summary>
        /// Возвращает ссылку для покупки билета.
        /// </summary>
        /// <param name="task">Запрашиваемый рейс.</param>
        /// <returns>Ссылка.</returns>
        public Task<string?> GetLinkToBuyTicketAsync(NotificationTask task);
    }
}