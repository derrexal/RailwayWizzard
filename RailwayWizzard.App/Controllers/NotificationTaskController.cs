using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Data;
using System.Globalization;
using RailwayWizzard.Robot.App;


namespace RailwayWizzard.App.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class NotificationTaskController : Controller
    {
        private readonly IBotApi _botApi;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly RailwayWizzardAppContext _context;

        public NotificationTaskController(
            RailwayWizzardAppContext context, 
            ILogger<NotificationTaskController> logger,
            IConfiguration configuration,
            IBotApi botApi)
        {
            _botApi = botApi;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }


        [HttpPost("CreateAndGetId")]
        public async Task<IActionResult> CreateAndGetId(NotificationTask stationInfo)
        {
            if (ModelState.IsValid)
            {
                stationInfo.CreationTime = DateTime.Now;
                stationInfo.IsActual = true;
                stationInfo.IsWorked = false;
                stationInfo.IsStopped = false;
                _context.Add(stationInfo);

                await _context.SaveChangesAsync();
                
                _logger.LogTrace($"Success create NotificationTask. Id:{stationInfo.Id} UserId:{stationInfo.UserId}");
                
                var adminId = _configuration.GetValue<long>("Telegram:AdminId");
                await _botApi.SendMessageForUser(
                    $"Новая активность в боте. Задача: {stationInfo.Id} {stationInfo.ToCustomString()}",adminId);
                
                return Ok(stationInfo.Id);
            }

            return BadRequest("Request param is no valid");
        }

        /// <summary>
        /// Устанавливает флаг IsStopped у конкретной сущности NotificationTask
        /// </summary>
        /// <param name="idNotificationTask"></param>
        /// <returns></returns>

        [HttpGet("SetIsStopped")]
        public async Task<IActionResult> SetIsStopped(int idNotificationTask)
        {
            if (ModelState.IsValid)
            {
                var currentTask = await _context.NotificationTask.FirstOrDefaultAsync(t => t.Id==idNotificationTask);
                
                if (currentTask is null) { return BadRequest($"Error search task from Id:{idNotificationTask}"); }
                
                currentTask.IsStopped = true;
                currentTask!.IsWorked = false;
                
                await _context.SaveChangesAsync();
                return Ok(currentTask.Id);
            }
            return BadRequest("Request param is no valid");
        }

        /// <summary>
        /// Получает список активных задач для конкретного пользователя
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetActiveByUser")]
        public async Task<IList<NotificationTask>> GetActiveByUser(long userId)
        {
            if (ModelState.IsValid)
            {
                var notificationTasksQuery = _context.NotificationTask
                .Where(u => u.IsActual)
                .Where(u=>u.IsStopped==false)
                .Where(u => u.UserId == userId)
                .AsNoTracking();

                var notificationTasks = await notificationTasksQuery.Select(u => new NotificationTask
                {
                    Id = u.Id,
                    ArrivalStation = u.ArrivalStation,
                    DepartureStation = u.DepartureStation,
                    TimeFrom = u.TimeFrom,
                    DateFromString = u.DateFrom.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
                }).ToListAsync();

                return notificationTasks;

            }
            throw new Exception("Request param is no valid");
        }
    }
}