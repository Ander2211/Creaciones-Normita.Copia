namespace Shared.Models
{
    public class UserDto
    {
        public string? UserName { get; set; }
        public List<string>? Roles { get; set; }
        public string? AuthToken { get; set; }
    }
}
