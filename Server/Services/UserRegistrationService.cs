using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Shared.Models;
using System.Security.Claims;

namespace Server.Services
{
    public class UserRegistrationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRegistrationService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<(bool Success, object Result)> RegisterUserAsync(RegisterRequest request, ClaimsPrincipal userPrincipal)
        {
            // si no manda rol, siempre será Cliente
            var role = string.IsNullOrEmpty(request.Role) ? "Cliente" : request.Role;

            // si alguien pide un rol distinto a Cliente ? solo admin puede hacerlo
            if (role != "Cliente")
            {
                if (!userPrincipal.Identity?.IsAuthenticated ?? true || !userPrincipal.IsInRole("Administrador"))
                {
                    return (false, new { Message = "Solo un administrador puede crear usuarios con roles distintos a Cliente." });
                }
            }

            // asegurarse de que el rol existe
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Dui = request.Dui,
                Email = request.Email,
                Nombre = request.Nombre,
                PhoneNumber = request.Telefono,
                Pais = request.Pais,
                FechaNac = request.FechaNac,
                FechaCreacion = DateTime.UtcNow
            };

            try
            {
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    return (false, result.Errors);

                await _userManager.AddToRoleAsync(user, role);

                return (true, new { Message = $"Usuario registrado correctamente con rol {role}." });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    var constraint = pgEx.ConstraintName;
                    string field = constraint switch
                    {
                        "IX_AspNetUsers_Dui" => "DUI",
                        "IX_AspNetUsers_PhoneNumber" => "Teléfono",
                        "IX_AspNetUsers_Email" => "Email",
                        _ => "Campo"
                    };

                    return (false, new[] { new { code = "DuplicateField", description = $"{field} ya está en uso." } });
                }
                throw;
            }
        }
    }
}