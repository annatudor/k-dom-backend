namespace KDomBackend.Models.DTOs.User
{
    public class UserLoginDto
    {
        public string Identifier { get; set; } = string.Empty; // email sau username
        public string Password { get; set; } = string.Empty;
    }

}
