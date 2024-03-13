using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        //Это тоже перевести бы во время)
        public string TimeFrom { get; set; }
        [Required]
        public long UserId { get; set; }
        public bool IsActual { get; set; }
        public DateTime CreationTime { get; set; }

        [Description("Эта задача уже в работе?")]
        public bool IsWorked { get; set; }

        [NotMapped]
        public long ArrivalStationCode { get; set; }
        [NotMapped]
        public long DepartureStationCode { get; set; }
    }
}