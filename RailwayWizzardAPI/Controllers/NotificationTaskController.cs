using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Data;
using RzdHack.Robot.Core;


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
                //await RunActiveTask();

                return Ok(stationInfo.Id);
            }

            return BadRequest("Request param is no valid");
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

