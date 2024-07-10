using Microsoft.AspNetCore.Mvc;
using RailwayWizzard.B2B;


namespace RailwayWizzard.App.Controllers
{
    //Контроллер для получения данных от сервисов РЖД
    [ApiController]
    [Route("[controller]")]
    public class BTwoBController : Controller
    {
        private readonly IPassRzd _passRzd;

        public BTwoBController(IPassRzd passRzd) =>
            _passRzd = passRzd;

        [HttpGet("GetStations")]
        public async Task<IActionResult> GetStations(string inputStation)
        {
            if (!ModelState.IsValid)
                return BadRequest("Request param is no valid");

            return Ok(await _passRzd.GetStations(inputStation));
        }

        [HttpGet("GetAvailableTimes")]
        public async Task<IActionResult> GetAvailableTimes(ScheduleDto scheduleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Request param is no valid");

            //подготовка данных
            scheduleDto.StationFrom = scheduleDto.StationFrom.ToUpper();
            scheduleDto.StationTo = scheduleDto.StationTo.ToUpper();

            return Ok(await _passRzd.GetAvailableTimes(scheduleDto));
            //Перенес оба метода из RZD АПИ в контроллер, проверил - все воркед. Осталось только удалить из бота и заменить все необходимые места. А так же возможно вынести и другую логику в робота
        }

        [HttpGet("GetStationValidate")]
        public async Task<IActionResult> GetStationValidate(string inputStation)
        {
            if (!ModelState.IsValid)
                return BadRequest("Request param is no valid");

            return Ok(await _passRzd.StationValidate(inputStation));
        }
    }
}
