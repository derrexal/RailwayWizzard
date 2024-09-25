using RailwayWizzard.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace RailwayWizzard.App.Dto.NotificationTask
{
    public class NotificationTaskDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string DepartureStation { get; set; }

        [Required]
        public string ArrivalStation { get; set; }

        [Required]
        public string TimeFrom { get; set; }

        [Description("Типы вагона")]
        public List<CarTypeEnum> CarTypes { get; set; }

        [Range(1, 10)]
        public short NumberSeats { get; set; }

        /// <summary>
        /// Дата в формате строки для отправки пользователю
        /// </summary>
        public string DateFromString { get; set; } = "";
    }
}
