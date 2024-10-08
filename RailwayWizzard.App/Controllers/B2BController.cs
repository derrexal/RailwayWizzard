﻿using Microsoft.AspNetCore.Mvc;
using RailwayWizzard.App.Dto.B2B;
using RailwayWizzard.App.Services.B2B;


namespace RailwayWizzard.App.Controllers
{
    //Контроллер для получения данных от сервисов РЖД
    [ApiController]
    [Route("[controller]")]
    public class B2BController : Controller
    {
        private readonly IB2BService _b2bService;

        public B2BController(IB2BService b2bService) =>
            _b2bService = b2bService;


        [HttpGet("GetAvailableTimes")]
        public async Task<IActionResult> GetAvailableTimes(RouteDto scheduleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            return Ok(await _b2bService.GetAvailableTimesAsync(scheduleDto));
        }

        [HttpGet("GetStationValidate")]
        public async Task<IActionResult> GetStationValidate(string stationName)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            return Ok(await _b2bService.StationValidateAsync(stationName));
        }
    }
}
