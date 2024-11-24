using Abp.Domain.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailwayWizzard.Core
{
    [Table("AppNotificationTasks")]
    public class NotificationTask : Entity
    {
        [Required]
        public string DepartureStation { get; set; } = null!;

        [Required]
        public string ArrivalStation { get; set; } = null!;

        [Required]
        public DateTime DepartureDateTime { get; set; }

        [Required]
        public long UserId { get; set; }
        
        public bool IsActual { get; set; }
        
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        
        //TODO: Сделать новое поле LastCheckResult в котором будет указано каков результат последней проверки (enum)
        
        [Description("Эта задача уже в работе?")]
        public bool IsWorked { get; set; }
        
        [Description("Поиск остановлен")]
        public bool IsStopped { get; set; }

        [Description("Типы вагона")]
        public List<CarTypeEnum> CarTypes { get; set; }

        [Range(1, 10)]
        public short NumberSeats { get; set; }

        public long ArrivalStationCode { get; set; } = 0;

        public long DepartureStationCode { get; set; } = 0;

        public string? TrainNumber { get; set; } = null!;

        public string LastResult { get; set; } = "";

        public string ToCustomString() =>
            $"{TrainNumber} {DepartureStation} - {ArrivalStation} {DepartureDateTime:t} {DepartureDateTime:yyyy-MM-dd}";
        
        public string ToBotString() =>
            $"<strong>{TrainNumber}</strong> {DepartureStation} - {ArrivalStation} {DepartureDateTime:t} {DepartureDateTime:yyyy-MM-dd}";
    }
}