namespace Shared.Models
{
    public class RegisterRequest
    {
        public string Dui { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;   
        public DateTime FechaNac { get; set; }
        public string Role { get; set; } = "Cliente";          
        public string Email { get; set; } = string.Empty;
    }
}