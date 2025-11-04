using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models; // Asegúrate de importar el modelo
using System.Security.Claims;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var username = User.Identity?.Name;
            var roles = User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();

            // Extrae el token de la cookie HttpOnly
            string? token = Request.Cookies["AuthToken"];

            var dto = new UserDto
            {
                UserName = username,
                Roles = roles,
                AuthToken = token // 👈 Aquí va el token
            };

            return Ok(dto);
        }
    }
}
