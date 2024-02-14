using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Data;
using RzdHack.Robot.Core;
using System.Globalization;
using RailwayWizzard.Core;
using RzdHack.Robot.App;


namespace RailwayWizzard.App.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class NotificationTaskController : Controller
    {

        private readonly RailwayWizzardAppContext _context;

        public NotificationTaskController(RailwayWizzardAppContext context)
        {
            _context = context;
        }


        [HttpPost("CreateAndGetId")]
        public async Task<IActionResult> CreateAndGetId(NotificationTask stationInfo)
        {
            if (ModelState.IsValid)
            {
                stationInfo.CreationTime = DateTime.Now;
                stationInfo.IsActual = true;
                _context.Add(stationInfo);

                await _context.SaveChangesAsync();
                //Все таки есть смысл вынести это в воркер, а отсюда н-р дергать его.
                //Т.к. при падении сервиса - следующий запуск будет только если другой пользователь создаст еще одну задачу
                await RunActiveTask();

                return Ok(stationInfo.Id);
            }

            return BadRequest("Request param is no valid");
        }

        private async Task RunActiveTask()
        {
            await UpdateIsActual();
            var activeNotificationTasks = await GetActive();
            foreach (var task in activeNotificationTasks)
            {
                //Дозаполняем кода городов
                task.ArrivalStationCode =
                    (await new StationInfoController(_context).GetByName(
                        new StationInfo { StationName = task.ArrivalStation})).ExpressCode;
                task.DepartureStationCode = 
                    (await new StationInfoController(_context).GetByName(   
                        new StationInfo { StationName = task.DepartureStation})).ExpressCode;

                var t = new Thread(() => new StepsUsingHttpClient().Notification(task));
                t.Start();
            }
        }

        ///TODO:Что если таких не будет? Метод сломается?
        /// <summary>
        /// Получает список задач со статусом "Актуально"
        /// </summary>
        /// <returns></returns>
        private async Task<IList<NotificationTask>> GetActive()
        {
            var notificationTasks = await _context.NotificationTask
                .Where(t => t.IsActual == true)
                .ToListAsync();
            return notificationTasks;
        }

        /// <summary>
        /// Обновляет поле "IsActual" если дата поездки уже в прошлом
        /// </summary>
        /// <returns></returns>
        private async Task UpdateIsActual()
        {
            var activeNotificationTasks = await GetActive();
            foreach (var activeNotificationTask in activeNotificationTasks)
            {
                DateTime itemDateFromDateTime = DateTime.ParseExact(
                    activeNotificationTask.DateFrom.ToShortDateString() + " " + activeNotificationTask.TimeFrom,
                    "dd.MM.yyyy HH:mm", 
                    CultureInfo.InvariantCulture);

                if (itemDateFromDateTime < DateTime.Now)
                {
                    activeNotificationTask.IsActual = false;
                    _context.NotificationTask.Update(activeNotificationTask);
                }
            }
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Получает все сущности из таблицы
        /// </summary>
        /// <returns></returns>
        private async Task<IList<NotificationTask>> GetAll()
        {
            var notificationTasks = await _context.NotificationTask.ToListAsync();
            return notificationTasks;
        }

    }
}

