using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RailwayWizzard.Common;

namespace RailwayWizzard.Core.NotificationTask
{
    [Table("AppNotificationTasks")]
    public class NotificationTask : Entity
    {
        /// <summary>
        /// Идентификатор станции отправления.
        /// </summary>
        [Required]
        public int DepartureStationId { get; init; }

        /// <summary>
        /// Идентификатор станции прибытия.
        /// </summary>
        [Required]
        public int ArrivalStationId { get; init; }

        /// <summary>
        /// Дата и время отправления.
        /// </summary>
        [Required]
        public DateTime DepartureDateTime { get; init; }

        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        [Required]
        public int CreatorId { get; init; }
        
        /// <summary>
        /// Дата создания.
        /// </summary>
        public DateTime Created { get; init; }
        
        /// <summary>
        /// Дата обновления состояния задачи.
        /// </summary>
        public DateTime Updated { get; set; }
        
        /// <summary>
        /// Флаг актуальности задачи.
        /// Выставляется на основе даты отправления относительно текущего времени.
        /// </summary>
        public bool IsActual { get; set; }
        
        /// <summary>
        /// Флаг выполнения задачи.
        /// Выставляется когда задача находится в процессе обработки в текущий момент времени.
        /// </summary>
        public bool IsProcess { get; set; }
        
        /// <summary>
        /// Флаг задачи которая была остановлена пользователем.
        /// </summary>
        public bool IsStopped { get; set; }

        /// <summary>
        /// Выбранные классы обслуживания.
        /// </summary>
        // TODO: переделать на нормальную один ко многим
        public List<CarType> CarTypes { get; init; } = null!;

        /// <summary>
        /// Количество необходимых мест.
        /// </summary>
        [Range(1, 10)]
        public short NumberSeats { get; init; }

        /// <summary>
        /// Номер поезда.
        /// </summary>
        public string? TrainNumber { get; set; }
        
        /// <summary>
        /// Преобразует задачу в строку для лога.
        /// </summary>
        /// <returns></returns>
        public string ToLogString() =>
            $"{TrainNumber} {DepartureStationId} - {ArrivalStationId} {DepartureDateTime:yyyy-MM-dd} {DepartureDateTime:t}";
        
        /// <summary>
        /// Проверяет задачу на актуальность по московскому времени.
        /// </summary>
        /// <returns>Результат проверки на актуальность задачи.</returns>
        public bool IsActuality() => DepartureDateTime > DateTimeExtensions.MoscowNow;
    }
}