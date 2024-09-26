using Microsoft.AspNetCore.Mvc;
using RailwayWizzard.App.Dto.NotificationTask;
using RailwayWizzard.App.Services.NotificationTasks;

namespace RailwayWizzard.App.Controllers
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

        /// <summary>
        /// Устанавливает флаг IsStopped у конкретной сущности NotificationTask
        /// </summary>
        /// <param name="notificationTaskId"></param>
        /// <returns></returns>

        [HttpGet("SetIsStopped")]
        public async Task<IActionResult> SetIsStopped(int notificationTaskId)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            var result = await _notificationTaskService.SetIsStoppedAsync(notificationTaskId);
            
            if (result is null)
                return BadRequest($"Error search task from Id:{notificationTaskId}");
            
            return Ok(result);
        }

        /// <summary>
        /// Получает список активных задач для конкретного пользователя
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetActiveByUser")]
        public async Task<IActionResult> GetActiveByUser(long userId)
        {
            if (!ModelState.IsValid)
                throw new Exception($"Request param is no valid: {ModelState}");

            var result = await _notificationTaskService.GetActiveByUserAsync(userId);

            return Ok(result);
        }
    }
}