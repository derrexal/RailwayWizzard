using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Data;
using RailwayWizzard.Core;


namespace RailwayWizzard.App.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class StationInfoController : Controller
    {

        private readonly RailwayWizzardAppContext _context;

        public StationInfoController(RailwayWizzardAppContext context)
        {
            _context = context;
        }

        [HttpGet("GetByName")]
        public async Task<StationInfo?> GetByName(StationInfo stationInfo)
        {
            return await _context.StationInfo.FirstOrDefaultAsync(s=>s.StationName==stationInfo.StationName);
        }

        [HttpPost("CreateOrUpdate")]
        public async Task<IActionResult> CreateOrUpdate(StationInfo stationInfo)
        {
            if (ModelState.IsValid)
            {
                var currentStationInfo = await _context.StationInfo.FirstOrDefaultAsync(u => u.ExpressCode == stationInfo.ExpressCode);
                if (currentStationInfo is null)
                    _context.Add(stationInfo);
                else
                {
                    currentStationInfo.StationName= stationInfo.StationName;
                    currentStationInfo.ExpressCode = stationInfo.ExpressCode;
                    _context.Update(currentStationInfo);
                }
                await _context.SaveChangesAsync();
                return Ok("Success StationInfo CreateOrUpdate");
            }

            return BadRequest("Request param is no valid");
        }
    }
}
