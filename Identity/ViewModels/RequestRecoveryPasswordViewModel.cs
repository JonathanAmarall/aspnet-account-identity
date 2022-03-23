using System.ComponentModel.DataAnnotations;

namespace Identity.ViewModels
{
    public class RequestRecoveryPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
