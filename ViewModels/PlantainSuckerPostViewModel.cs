using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ade_Farming.ViewModels
{
    public class PlantainSuckerPostViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public IFormFile? Image { get; set; }  // For file upload
        public string? ExistingImagePath { get; set; }  // For edit
    }
}
