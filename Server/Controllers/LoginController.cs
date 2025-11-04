using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services; // Importa el servicio
using Shared.Models;
using Npgsql; // Agrega esta línea para capturar excepciones de Npgsql

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;
        private readonly JwtService _jwtService; // Agrega el servicio

        public LoginController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            JwtService jwtService) // Inyecta el servicio
        {
            _userManager = userManager;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<ApplicationUser>();
            _jwtService = jwtService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Buscar usuario por UserName
                var user = await _userManager.FindByNameAsync(request.UserName);
                if (user == null)
                    return Unauthorized("Usuario o contraseña incorrecta");

                // Verificar contraseña
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, request.Password);
                if (result == PasswordVerificationResult.Failed)
                    return Unauthorized("Usuario o contraseña incorrecta");

                var roles = await _userManager.GetRolesAsync(user);

                // Usa el servicio para generar el token
                var jwtString = _jwtService.GenerateToken(user, roles, out DateTime? expires);

                // Guardar JWT en HttpOnly cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Cambia a true para HTTPS en producción
                    SameSite = SameSiteMode.Lax,
                    Expires = expires
                };
                Response.Cookies.Append("AuthToken", jwtString, cookieOptions);

                // Respuesta opcional con info mínima
                return Ok(new LoginResponse
                {
                    Token = jwtString,
                    Expiration = expires
                });
            }
            catch (NpgsqlException)
            {
                // Error de conexión a la base de datos
                return StatusCode(503, "Error de conexión al servidor");
            }
            catch (Exception)
            {
                // Otros errores
                return StatusCode(500, "Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                Response.Cookies.Append("AuthToken", "", new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(-1),
                    HttpOnly = true,
                    Secure = true, // Cambia a true para HTTPS en producción
                    SameSite = SameSiteMode.Lax
                });

                return Ok(new { Message = "Logout exitoso" });
            }

            return Ok(new { Message = "No fue posible desloguearse" });
        }
    }
    
}
