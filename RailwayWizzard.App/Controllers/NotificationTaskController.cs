using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Data;
using System.Globalization;

namespace RailwayWizzard.App.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class NotificationTaskController : Controller
    {
        private readonly ILogger _logger;
        private readonly RailwayWizzardAppContext _context;

        public NotificationTaskController(
            RailwayWizzardAppContext context, 
            ILogger<NotificationTaskController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("CreateAndGetId")]
        public async Task<IActionResult> CreateAndGetId(NotificationTask stationInfo)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            stationInfo.CreationTime = DateTime.Now;
            stationInfo.IsActual = true;
            stationInfo.IsWorked = false;
            stationInfo.IsStopped = false;
            _context.Add(stationInfo);

            await _context.SaveChangesAsync();
            _logger.LogTrace($"Success create NotificationTask. Id:{stationInfo.Id} UserId:{stationInfo.UserId}");
            return Ok(stationInfo.Id);
        }

        /// <summary>
        /// Устанавливает флаг IsStopped у конкретной сущности NotificationTask
        /// </summary>
        /// <param name="idNotificationTask"></param>
        /// <returns></returns>

        [HttpGet("SetIsStopped")]
        public async Task<IActionResult> SetIsStopped(int idNotificationTask)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            var currentTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id==idNotificationTask);
            
            if (currentTask is null) { return BadRequest($"Error search task from Id:{idNotificationTask}"); }
            
            currentTask.IsStopped = true;
            currentTask!.IsWorked = false;
            
            await _context.SaveChangesAsync();
            return Ok(currentTask.Id);
        }

        /// <summary>
        /// Получает список активных задач для конкретного пользователя
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetActiveByUser")]
        public async Task<IList<NotificationTask>> GetActiveByUser(long userId)
        {
            if (!ModelState.IsValid)
                throw new Exception($"Request param is no valid: {ModelState}");

            var notificationTasksQuery = _context.NotificationTask
            .Where(u => u.IsActual)
            .Where(u=>!u.IsStopped)
            .Where(u => u.UserId == userId)
            .AsNoTracking();

            var notificationTasks = await notificationTasksQuery.Select(u => new NotificationTask
            {
                Id = u.Id,
                ArrivalStation = u.ArrivalStation,
                DepartureStation = u.DepartureStation,
                TimeFrom = u.TimeFrom,
                CarTypes = u.CarTypes,
                NumberSeats = u.NumberSeats,
                DateFromString = u.DateFrom.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
            }).ToListAsync();

            return notificationTasks;
        }
    }
}