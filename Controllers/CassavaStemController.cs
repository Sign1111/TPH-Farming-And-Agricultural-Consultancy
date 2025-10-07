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
    [Authorize] // Require login for all actions by default
    public class CassavaStemController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMongoCollection<CassavaStemPost> _posts;
        private readonly ILogger<CassavaStemController> _logger;

        public CassavaStemController(
            UserManager<ApplicationUser> userManager,
            IMongoDatabase database,
            ILogger<CassavaStemController> logger)
        {
            _userManager = userManager;
            _posts = database.GetCollection<CassavaStemPost>("CassavaStemPosts");
            _logger = logger;
        }

        // ================== CREATE ==================
        [Authorize(Policy = "SellerOnly")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CassavaStemPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _logger.LogWarning("ModelState invalid: {Errors}", errors);
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found or not logged in.";
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

                var post = new CassavaStemPost
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImagePath = imagePath,
                    SellerId = user.Id.ToString(),
                    SellerName = user.FullName,
                    SellerEmail = user.Email, // <-- store email here
                    CreatedAt = DateTime.UtcNow
                };

                await _posts.InsertOneAsync(post);
                TempData["SuccessMessage"] = "Post created successfully!";
                return RedirectToAction("MyPosts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating CassavaStemPost");
                TempData["ErrorMessage"] = "Failed to create post: " + ex.Message;
                return View(model);
            }
        }

        // ================== READ (My Posts) ==================
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> MyPosts()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Login", "Account");
                }

                var filter = Builders<CassavaStemPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());
                var myPosts = await _posts.Find(filter).ToListAsync();

                return View(myPosts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching MyPosts");
                TempData["ErrorMessage"] = "Failed to load your posts.";
                return View(Enumerable.Empty<CassavaStemPost>());
            }
        }

        // ================== EDIT ==================
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var filter = Builders<CassavaStemPost>.Filter.Eq(p => p.Id, id) &
                             Builders<CassavaStemPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());

                var post = await _posts.Find(filter).FirstOrDefaultAsync();
                if (post == null) return NotFound();

                var model = new CassavaStemPostViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    Description = post.Description,
                    ExistingImagePath = post.ImagePath
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Edit view for Id={Id}", id);
                TempData["ErrorMessage"] = "Failed to load post for editing.";
                return RedirectToAction("MyPosts");
            }
        }

        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CassavaStemPostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var filter = Builders<CassavaStemPost>.Filter.Eq(p => p.Id, id) &
                             Builders<CassavaStemPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());

                var update = Builders<CassavaStemPost>.Update
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

                    var imagePath = "/Images/Posts/" + fileName;
                    update = update.Set(p => p.ImagePath, imagePath);
                }

                await _posts.UpdateOneAsync(filter, update);
                TempData["SuccessMessage"] = "Post updated successfully!";
                return RedirectToAction("MyPosts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating CassavaStemPost Id={Id}", id);
                TempData["ErrorMessage"] = "Failed to update post: " + ex.Message;
                return View(model);
            }
        }

        // ================== DELETE ==================
        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var filter = Builders<CassavaStemPost>.Filter.Eq(p => p.Id, id) &
                             Builders<CassavaStemPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());

                await _posts.DeleteOneAsync(filter);
                TempData["SuccessMessage"] = "Post deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting CassavaStemPost Id={Id}", id);
                TempData["ErrorMessage"] = "Failed to delete post: " + ex.Message;
            }

            return RedirectToAction("MyPosts");
        }

        // ================== INDEX (All posts) ==================
        public async Task<IActionResult> Index()
        {
            try
            {
                var posts = await _posts.Find(Builders<CassavaStemPost>.Filter.Empty).ToListAsync();
                return View(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all posts");
                TempData["ErrorMessage"] = "Failed to load posts.";
                return View(Enumerable.Empty<CassavaStemPost>());
            }
        }
    }
}
