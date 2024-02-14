using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace RzdHack.Robot.Core
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
        public string TimeFrom { get; set; }
        [Required]
        public long UserId { get; set; }
        public bool IsActual { get; set; }
        public int TotalCountNotification { get; set; }
        public DateTime CreationTime { get; set; }

    }
}