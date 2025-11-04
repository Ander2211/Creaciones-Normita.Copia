using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models; // Aquí están RegisterUserRequest, RegisterUserResponse
using Server.Services; // Importa el servicio

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly UserRegistrationService _registrationService;

        public RegisterController(UserRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpPost]
        [AllowAnonymous] // cualquiera puede pegarle
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, result) = await _registrationService.RegisterUserAsync(request, User);

            if (success)
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}
