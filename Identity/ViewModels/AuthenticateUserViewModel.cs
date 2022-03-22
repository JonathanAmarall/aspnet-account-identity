using System.ComponentModel.DataAnnotations;

namespace Identity.ViewModels
{
    public class AuthenticateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }


        [Required]
        [MinLength(6)]
        public string ConfirmPassword { get; set; }
    }
}
