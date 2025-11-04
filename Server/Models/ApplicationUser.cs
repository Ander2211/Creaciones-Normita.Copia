using Microsoft.AspNetCore.Identity;

namespace Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Dui { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public DateTime FechaNac { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
