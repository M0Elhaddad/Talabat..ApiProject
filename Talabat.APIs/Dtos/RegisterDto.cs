using System.ComponentModel.DataAnnotations;

namespace Talabat.APIs.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string DisplayName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]

        public string PhoneNumber { get; set; }

        [Required]
        [RegularExpression("(?=^.{6,10}$)(?=.*\\d)(?=.*[a-z])(?=.*[!@#$%^amp;*()_+}{&quot;:;'?/&gt;.&lt;,])(?!.*\\s).*$",ErrorMessage = "Password must have 1 UpperCase, 1 LoweCase, 1 number, 1 non alphanumeric and at least 6 characters")]
        public string Password { get; set; }
    }
}
