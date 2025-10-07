using System.ComponentModel.DataAnnotations;
using Ade_Farming.Attributes;

namespace Ade_Farming.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Register as Seller?")]
        public bool IsSeller { get; set; }

        [RequiredIfSeller("IsSeller", ErrorMessage = "Address is required for sellers.")]
        public string? Address { get; set; }

        [RequiredIfSeller("IsSeller", ErrorMessage = "Contact is required for sellers.")]
        public string? Contact { get; set; }

        [RequiredIfSeller("IsSeller", ErrorMessage = "Gender is required for sellers.")]
        public string? Gender { get; set; }

        [RequiredIfSeller("IsSeller", ErrorMessage = "Answer is required for Question1.")]
        public string? Question1 { get; set; }

        [RequiredIfSeller("IsSeller", ErrorMessage = "Answer is required for Question2.")]
        public string? Question2 { get; set; }

    }
}
