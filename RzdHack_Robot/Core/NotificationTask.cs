using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace RzdHack_Robot.Core
{
    [Table("AppNotificationTasks")]
    public class NotificationTask:Entity
    {
        public string DepartureStation { get; set; }
        public string ArrivalStation { get; set; }
        public string DateFrom { get; set; }
        public string TimeFrom { get; set; }
        public long UserId { get; set; }
        public bool IsActual { get; set; }
        public int TotalCountNotification { get; set; }
        public DateTime CreationTime { get; set; }

    }
}