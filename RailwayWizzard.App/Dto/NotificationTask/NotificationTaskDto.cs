using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RailwayWizzard.App.Dto.NotificationTask
{
    public class NotificationTaskDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string DepartureStation { get; set; } = null!;

        [Required]
        public string ArrivalStation { get; set; } = null!;

        [Required]
        public string TimeFrom { get; set; } = null!;

        [Description("Типы вагона")]
        public string CarTypes { get; set; } = null!;

        [Range(1, 10)]
        public short NumberSeats { get; set; }

        /// <summary>
        /// Дата в формате строки для отправки пользователю
        /// </summary>
        public string DateFromString { get; set; } = "";

        public string Updated { get; set; } = "";
    }
}
