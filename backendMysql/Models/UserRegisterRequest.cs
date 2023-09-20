using System.ComponentModel.DataAnnotations;

namespace backendMysql.Models
{
    public class UserRegisterRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters, dude!")]
        public string Password { get; set; } = string.Empty;
        
        
    }
}
