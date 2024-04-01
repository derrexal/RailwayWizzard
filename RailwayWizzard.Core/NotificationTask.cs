using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Abp.Domain.Entities;


namespace RailwayWizzard.Core
{
    [Table("AppNotificationTasks")]
    public class NotificationTask: Entity
    {
        [Required]
        public string DepartureStation { get; set; }
        [Required]
        public string ArrivalStation { get; set; }
        [Required]
        public DateTime DateFrom { get; set; }
        [Required]
        //todo:Это тоже перевести бы во время)
        public string TimeFrom { get; set; }
        [Required]
        public long UserId { get; set; }
        public bool IsActual { get; set; }
        public DateTime CreationTime { get; set; }

        [Description("Эта задача уже в работе?")]
        public bool IsWorked { get; set; }
        [Description("Поиск остановлен")]
        public bool IsStopped { get; set; }

        [NotMapped]
        public long ArrivalStationCode { get; set; }
        [NotMapped]
        public long DepartureStationCode { get; set; }
        
        //TODO: вынести в DTO, сделал nulalble так как была ошибка при создании таска из-за отсутствия этого параметра в запросе
        /// <summary>
        /// Дата в формате строки для отправки пользователю
        /// </summary>
        [NotMapped]
        public string? DateFromString { get; set; }

        /// <summary>
        /// Формирование строки из некоторых полей объекта
        /// </summary>
        /// <returns></returns>
        public string ToCustomString()
        {
            return $"{DepartureStation} - {ArrivalStation} {TimeFrom} {DateFrom.ToString("dd.MM.yyy", CultureInfo.InvariantCulture)}";
        }
    }
}