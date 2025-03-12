using Microsoft.AspNetCore.Mvc;
using RailwayWizzard.Application.Dto.NotificationTask;
using RailwayWizzard.Application.Services.NotificationTasks;

namespace RailwayWizzard.Application.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class NotificationTaskController : Controller
    {
        private readonly INotificationTaskService _notificationTaskService;

        public NotificationTaskController(INotificationTaskService notificationTaskService)
        {
            _notificationTaskService = notificationTaskService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateNotificationTaskDto createNotificationTaskDto)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            return Ok(await _notificationTaskService.CreateAsync(createNotificationTaskDto));
        }

        [HttpGet("SetIsStopped")]
        public async Task<IActionResult> SetIsStopped(int notificationTaskId)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            var result = await _notificationTaskService.SetIsStoppedAsync(notificationTaskId);

            // TODO: перенести на уровень репозитория.
            if (result is null)
                return BadRequest($"Error search task from Id:{notificationTaskId}");

            return Ok(result);
       }

        [HttpGet("GetActiveByUser")]
        public async Task<IActionResult> GetActiveByUser(long telegramUserId)
        {
            if (!ModelState.IsValid)
                throw new Exception($"Request param is no valid: {ModelState}");

            var result = await _notificationTaskService.GetActiveByUserAsync(telegramUserId);

            return Ok(result);
        }
                
        [HttpGet("GetPopularCities")]
        public async Task<IActionResult> GetPopularCities(long telegramUserId)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");
            
            var result = await _notificationTaskService.GetPopularCitiesByUserAsync(telegramUserId);
            
            return Ok(result);
        }
    }
}