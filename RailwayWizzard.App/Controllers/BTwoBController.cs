using Microsoft.AspNetCore.Mvc;
using RailwayWizzard.B2B;


namespace RailwayWizzard.App.Controllers
{
    //Контроллер для получения данных от сервисов РЖД
    [ApiController]
    [Route("[controller]")]
    public class BTwoBController : Controller
    {
        private readonly IB2BService _b2bService;

        //TODO: rename controller for B2B service
        public BTwoBController(IB2BService b2bService) =>
            _b2bService = b2bService;


        [HttpGet("GetAvailableTimes")]
        public async Task<IActionResult> GetAvailableTimes(ScheduleDto scheduleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            //подготовка данных
            scheduleDto.StationFrom = scheduleDto.StationFrom.ToUpper();
            scheduleDto.StationTo = scheduleDto.StationTo.ToUpper();

            return Ok(await _b2bService.GetAvailableTimes(scheduleDto));
        }   

        [HttpGet("GetStationValidate")]
        public async Task<IActionResult> GetStationValidate(string inputStation)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            return Ok(await _b2bService.StationValidate(inputStation));
        }
    }
}
