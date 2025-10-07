using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ade_Farming.ViewModels
{
    public class CassavaTubersPostViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public IFormFile? Image { get; set; }

        public string? ExistingImagePath { get; set; }
    }
}
