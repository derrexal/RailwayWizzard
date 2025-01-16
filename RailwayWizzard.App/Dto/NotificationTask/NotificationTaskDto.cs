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

        [Description("Типы вагона")]
        public string CarTypes { get; set; } = null!;

        [Range(1, 10)]
        public short NumberSeats { get; set; }

        public string? TrainNumber { get; set; } = null!;

        public string DateFromString { get; set; } = "";

        public string Updated { get; set; } = "";
    }
}
