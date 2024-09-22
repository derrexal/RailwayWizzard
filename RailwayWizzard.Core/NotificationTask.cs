using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Abp.Domain.Entities;


namespace RailwayWizzard.Core
{
    [Table("AppNotificationTasks")]
    public class NotificationTask : Entity
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

        [Description("Типы вагона")]
        public List<CarTypeEnum> CarTypes { get; set; }

        [Range(1, 10)]
        public short NumberSeats { get; set; }


        [NotMapped]
        public long ArrivalStationCode { get; set; }
        [NotMapped]
        public long DepartureStationCode { get; set; }

        /// <summary>
        /// Дата в формате строки для отправки пользователю
        /// </summary>
        [NotMapped]
        public string DateFromString
        {
            get
            {
                return DateFrom.ToString("dd.MM.yyy", CultureInfo.InvariantCulture);
            }
        }

        public string LastResult { get; set; } = "";

        public string ToCustomString()
        {
            return $"{DepartureStation} - {ArrivalStation} {TimeFrom} {DateFromString}";
        }
    }
}