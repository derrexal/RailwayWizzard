using RailwayWizzard.Core;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RailwayWizzard.App.Dto.NotificationTask
{
    public class CreateNotificationTaskDto
    {
        [Required]
        public string DepartureStation { get; set; } = null!;

        [Required]
        public string ArrivalStation { get; set; } = null!; 

        [Required]
        public DateTime DateFrom { get; set; }

        [Required]
        //todo:Это тоже перевести бы во время)
        public string TimeFrom { get; set; } = null!;

        [Required]
        public long UserId { get; set; }

        [Description("Типы вагона")]
        //[EnumDataType(typeof(CarTypeEnum))]
        public List<CarTypeEnum> CarTypes { get; set; } = null!;    

        [Range(1, 10)]
        public short NumberSeats { get; set; }
    }
}
