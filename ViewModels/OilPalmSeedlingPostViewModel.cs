using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ade_Farming.ViewModels
{
    public class OilPalmSeedlingPostViewModel
    {
        public string? Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public IFormFile? Image { get; set; }

        public string? ExistingImagePath { get; set; }
    }
}
