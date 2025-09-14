using Microsoft.AspNetCore.Mvc;
using RailwayWizzard.Application.Dto.User;
using RailwayWizzard.Application.Services.Users;
using RailwayWizzard.Common;

namespace RailwayWizzard.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = Ensure.NotNull(userService);
        }

        [HttpPost("CreateOrUpdate")]
        public async Task<IActionResult> CreateOrUpdate(CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            //TODO: по хорошему должен возвращать юзера. Да и вообще переделать концепцию... Че за колхозный метод.
            await _userService.CreateOrUpdateAsync(createUserDto);

            return Ok("Success User Create");
        }
    }
}