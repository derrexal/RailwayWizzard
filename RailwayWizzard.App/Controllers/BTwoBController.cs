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
                return BadRequest($"Request param is no valid: {ModelState}");

            return Ok(await _passRzd.GetStations(inputStation));
        }

        [HttpGet("GetAvailableTimes")]
        public async Task<IActionResult> GetAvailableTimes(ScheduleDto scheduleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            //подготовка данных
            scheduleDto.StationFrom = scheduleDto.StationFrom.ToUpper();
            scheduleDto.StationTo = scheduleDto.StationTo.ToUpper();

            return Ok(await _passRzd.GetAvailableTimes(scheduleDto));
        }   

        [HttpGet("GetStationValidate")]
        public async Task<IActionResult> GetStationValidate(string inputStation)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            return Ok(await _passRzd.StationValidate(inputStation));
        }
    }
}
