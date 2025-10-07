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
    public class CocoaPalmSeedlingController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMongoCollection<CocoaPalmSeedlingPost> _posts;
        private readonly ILogger<CocoaPalmSeedlingController> _logger;

        public CocoaPalmSeedlingController(UserManager<ApplicationUser> userManager,
            IMongoDatabase database,
            ILogger<CocoaPalmSeedlingController> logger)
        {
            _userManager = userManager;
            _posts = database.GetCollection<CocoaPalmSeedlingPost>("CocoaPalmSeedlingPosts");
            _logger = logger;
        }

        [Authorize(Policy = "SellerOnly")]
        public IActionResult Create() => View();

        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CocoaPalmSeedlingPostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Login", "Account");
                }

                string imagePath = null;
                if (model.Image != null && model.Image.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Posts");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await model.Image.CopyToAsync(stream);
                    imagePath = "/Images/Posts/" + fileName;
                }

                var post = new CocoaPalmSeedlingPost
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImagePath = imagePath,
                    SellerId = user.Id.ToString(),
                    SellerName = user.FullName,
                    SellerEmail = user.Email,
                    CreatedAt = DateTime.UtcNow
                };

                await _posts.InsertOneAsync(post);
                TempData["SuccessMessage"] = "Post created successfully!";
                return RedirectToAction("MyPosts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating CocoaPalmSeedlingPost");
                TempData["ErrorMessage"] = "Failed to create post: " + ex.Message;
                return View(model);
            }
        }

        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> MyPosts()
        {
            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<CocoaPalmSeedlingPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());
            var posts = await _posts.Find(filter).ToListAsync();
            return View(posts);
        }

        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<CocoaPalmSeedlingPost>.Filter.Eq(p => p.Id, id) &
                         Builders<CocoaPalmSeedlingPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());
            var post = await _posts.Find(filter).FirstOrDefaultAsync();
            if (post == null) return NotFound();

            var model = new CocoaPalmSeedlingPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                ExistingImagePath = post.ImagePath
            };
            return View(model);
        }

        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CocoaPalmSeedlingPostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<CocoaPalmSeedlingPost>.Filter.Eq(p => p.Id, id) &
                         Builders<CocoaPalmSeedlingPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());

            var update = Builders<CocoaPalmSeedlingPost>.Update
                .Set(p => p.Title, model.Title)
                .Set(p => p.Description, model.Description);

            if (model.Image != null && model.Image.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Posts");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.Image.CopyToAsync(stream);
                update = update.Set(p => p.ImagePath, "/Images/Posts/" + fileName);
            }

            await _posts.UpdateOneAsync(filter, update);
            TempData["SuccessMessage"] = "Post updated successfully!";
            return RedirectToAction("MyPosts");
        }

        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<CocoaPalmSeedlingPost>.Filter.Eq(p => p.Id, id) &
                         Builders<CocoaPalmSeedlingPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());
            await _posts.DeleteOneAsync(filter);
            TempData["SuccessMessage"] = "Post deleted successfully!";
            return RedirectToAction("MyPosts");
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _posts.Find(Builders<CocoaPalmSeedlingPost>.Filter.Empty).ToListAsync();
            return View(posts);
        }
    }
}
