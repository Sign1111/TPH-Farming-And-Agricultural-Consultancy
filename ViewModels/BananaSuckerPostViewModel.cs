using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ade_Farming.ViewModels
{
    public class BananaSuckerPostViewModel
    {
        public string? Id { get; set; }  // For Edit operations

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public IFormFile? Image { get; set; }  // For file upload

        public string? ExistingImagePath { get; set; }  // To show existing image in Edit

        
    }
}
