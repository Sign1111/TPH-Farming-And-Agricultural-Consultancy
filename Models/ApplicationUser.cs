using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ade_Farming.Models
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public string? FullName { get; set; }
        [Display(Name = "Register as Seller?")]
        public bool IsSeller { get; set; }
        public string? Address { get; set; }
        public string? Contact { get; set; }
        public string? Gender { get; set; }
        public string? Question1 { get; set; }
        public string? Question2 { get; set; }

    }

    [CollectionName("Roles")]
    public class ApplicationRole : MongoIdentityRole<Guid>
    {
        public ApplicationRole() : base() { }

        public ApplicationRole(string roleName) : base(roleName) { }
    }
}
