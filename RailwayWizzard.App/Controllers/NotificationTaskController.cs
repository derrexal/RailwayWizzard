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

        public NotificationTaskController(RailwayWizzardAppContext context, ILogger<NotificationTaskController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [HttpPost("CreateAndGetId")]
        public async Task<IActionResult> CreateAndGetId(NotificationTask stationInfo)
        {
            if (ModelState.IsValid)
            {
                stationInfo.CreationTime = DateTime.Now;
                stationInfo.IsActual = true;
                stationInfo.IsWorked = false;
                _context.Add(stationInfo);

                await _context.SaveChangesAsync();
                _logger.LogTrace($"Success create NotificationTask. Id:{stationInfo.Id} UserId:{stationInfo.UserId}");
                return Ok(stationInfo.Id);
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