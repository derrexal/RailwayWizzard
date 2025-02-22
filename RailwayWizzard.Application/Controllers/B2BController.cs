using Microsoft.AspNetCore.Mvc;
using RailwayWizzard.Application.Dto.B2B;
using RailwayWizzard.Application.Services.B2B;


namespace RailwayWizzard.Application.Controllers
{
    //Контроллер для получения данных от сервисов РЖД
    [ApiController]
    [Route("[controller]")]
    public class B2BController : Controller
    {
        private readonly IB2BService _b2BService;

        public B2BController(IB2BService b2BService) =>
            _b2BService = b2BService;


        [HttpGet("GetAvailableTimes")]
        public async Task<IActionResult> GetAvailableTimes(RouteDto scheduleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            return Ok(await _b2BService.GetAvailableTimesAsync(scheduleDto));
        }

        [HttpGet("GetStationValidate")]
        public async Task<IActionResult> GetStationValidate(string stationName)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            return Ok(await _b2BService.StationValidateAsync(stationName));
        }
    }
}
