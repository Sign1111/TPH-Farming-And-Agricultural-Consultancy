using Ade_Farming.Models;
using Ade_Farming.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ade_Farming.Controllers
{
    [Authorize]
    public class PlantainSuckersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMongoCollection<PlantainSuckerPost> _posts;

        public PlantainSuckersController(UserManager<ApplicationUser> userManager, IMongoDatabase database)
        {
            _userManager = userManager;
            _posts = database.GetCollection<PlantainSuckerPost>("PlantainSuckerPosts");
        }

        // CREATE GET
        [Authorize(Policy = "SellerOnly")]
        public IActionResult Create() => View();

        // CREATE POST
        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlantainSuckerPostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            string imagePath = null;

            if (model.Image != null && model.Image.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Posts");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.Image.CopyToAsync(stream);
                imagePath = "/Images/Posts/" + fileName;
            }

            var post = new PlantainSuckerPost
            {
                Title = model.Title,
                Description = model.Description,
                ImagePath = imagePath,
                SellerId = user.Id.ToString(),
                SellerName = user.FullName,
                CreatedAt = DateTime.UtcNow
            };

            await _posts.InsertOneAsync(post);
            TempData["SuccessMessage"] = "Post created successfully!";
            return RedirectToAction("MyPosts");
        }

        // READ: Seller's Posts (MyPosts)
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> MyPosts()
        {
            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<PlantainSuckerPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());
            var myPosts = await _posts.Find(filter).ToListAsync();
            return View(myPosts);
        }

        // INDEX: All posts for everyone
        public async Task<IActionResult> Index()
        {
            var posts = await _posts.Find(Builders<PlantainSuckerPost>.Filter.Empty).ToListAsync();
            return View(posts);
        }

        // EDIT GET
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<PlantainSuckerPost>.Filter.Eq(p => p.Id, id) &
                         Builders<PlantainSuckerPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());

            var post = await _posts.Find(filter).FirstOrDefaultAsync();
            if (post == null) return NotFound();

            var model = new PlantainSuckerPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                ExistingImagePath = post.ImagePath
            };

            return View(model);
        }

        // EDIT POST
        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PlantainSuckerPostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<PlantainSuckerPost>.Filter.Eq(p => p.Id, id) &
                         Builders<PlantainSuckerPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());

            var update = Builders<PlantainSuckerPost>.Update
                .Set(p => p.Title, model.Title)
                .Set(p => p.Description, model.Description);

            if (model.Image != null && model.Image.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Posts");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.Image.CopyToAsync(stream);

                var imagePath = "/Images/Posts/" + fileName;
                update = update.Set(p => p.ImagePath, imagePath);
            }

            await _posts.UpdateOneAsync(filter, update);
            TempData["SuccessMessage"] = "Post updated successfully!";
            return RedirectToAction("MyPosts");
        }

        // DELETE
        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<PlantainSuckerPost>.Filter.Eq(p => p.Id, id) &
                         Builders<PlantainSuckerPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());

            await _posts.DeleteOneAsync(filter);
            TempData["SuccessMessage"] = "Post deleted successfully!";
            return RedirectToAction("MyPosts");
        }
    }
}
