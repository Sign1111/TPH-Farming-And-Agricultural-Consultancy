using System.ComponentModel.DataAnnotations;

namespace Ade_Farming.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
