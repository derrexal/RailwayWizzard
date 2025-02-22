namespace RailwayWizzard.Application.Dto.B2B
{
    /// <summary>
    /// Модель для получения информации по маршруту.
    /// </summary>
    public class RouteDto
    {
        /// <summary>
        /// Наименование станции отправления.
        /// </summary>
        public string DepartureStationName { get; set; } = null!;

        /// <summary>
        /// Наименование станции прибытия.
        /// </summary>
        public string ArrivalStationName { get; set; } = null!;

        /// <summary>
        /// Дата отправления.
        /// </summary>
        public DateTime DepartureDate { get; set; }
    }
}
