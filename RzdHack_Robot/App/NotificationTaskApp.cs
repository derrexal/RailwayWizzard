using RzdHack_Robot.Core;

namespace RzdHack_Robot.App
{
    public class NotificationTaskApp
    {
        public NotificationTaskApp()
        {
            this.Initialization();
        }

        /// <summary>
        /// Актуализируем статус записей в базе
        /// Выставляем статус "Не актуально" после того как дата отправления прошла
        /// </summary>
        public void Initialization()
        {
            try
            {
                using(ApplicationContext db  = new ApplicationContext())
                {
                    var currentDate = DateTime.Now;
                    var allList = this.GetAll();
                    foreach (var item in allList)
                    {
                        if (item != null)
                        {
                            var itemDateFromDateTime = DateTime.Parse(item.DateFrom + " " + item.TimeFrom);
                            if (itemDateFromDateTime < currentDate) 
                                item.IsActual = false;
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch { throw; }
        }

        public IList<NotificationTask> GetAll()
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    var notificationTasks = db.NotificationTasks.ToList();
                    return notificationTasks;
                }
            }
            catch { throw; }
        }

        public IList<NotificationTask> GetActive()
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    var notificationTasks = db.NotificationTasks
                        .Where(t=>t.IsActual==true)
                        .ToList();
                    return notificationTasks;
                }

            }
            catch { throw; }
        }
    }
}
