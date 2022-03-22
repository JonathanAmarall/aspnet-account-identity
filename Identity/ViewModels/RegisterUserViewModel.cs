using System.ComponentModel.DataAnnotations;

namespace Identity.ViewModels
{
    public class RegisterUserViewModel
    {
        [Required]
        public string UserName { get;  set; }

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
