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
    public class CassavaTubersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMongoCollection<CassavaTubersPost> _posts;

        public CassavaTubersController(UserManager<ApplicationUser> userManager, IMongoDatabase database)
        {
            _userManager = userManager;
            _posts = database.GetCollection<CassavaTubersPost>("CassavaTubersPosts");
        }

        // GET: Create
        [Authorize(Policy = "SellerOnly")]
        public IActionResult Create() => View();

        // POST: Create
        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CassavaTubersPostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

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
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "CassavaTubers");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.Image.CopyToAsync(stream);
                imagePath = "/Images/CassavaTubers/" + fileName;
            }

            var post = new CassavaTubersPost
            {
                Title = model.Title,
                Description = model.Description,
                ImagePath = imagePath,
                SellerId = user.Id.ToString(),
                SellerName = user.FullName,
                CreatedAt = DateTime.UtcNow
            };

            await _posts.InsertOneAsync(post);
            TempData["SuccessMessage"] = "Cassava Tubers post created successfully!";
            return RedirectToAction("MyPosts");
        }

        // GET: MyPosts
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> MyPosts()
        {
            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<CassavaTubersPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());
            var myPosts = await _posts.Find(filter).ToListAsync();
            return View(myPosts);
        }

        // GET: Edit
        [Authorize(Policy = "SellerOnly")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<CassavaTubersPost>.Filter.Eq(p => p.Id, id) &
                         Builders<CassavaTubersPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());

            var post = await _posts.Find(filter).FirstOrDefaultAsync();
            if (post == null) return NotFound();

            var model = new CassavaTubersPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                ExistingImagePath = post.ImagePath
            };
            return View(model);
        }

        // POST: Edit
        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CassavaTubersPostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<CassavaTubersPost>.Filter.Eq(p => p.Id, id) &
                         Builders<CassavaTubersPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());

            var update = Builders<CassavaTubersPost>.Update
                .Set(p => p.Title, model.Title)
                .Set(p => p.Description, model.Description);

            if (model.Image != null && model.Image.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "CassavaTubers");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.Image.CopyToAsync(stream);
                update = update.Set(p => p.ImagePath, "/Images/CassavaTubers/" + fileName);
            }

            await _posts.UpdateOneAsync(filter, update);
            TempData["SuccessMessage"] = "Post updated successfully!";
            return RedirectToAction("MyPosts");
        }

        // POST: Delete
        [Authorize(Policy = "SellerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            var filter = Builders<CassavaTubersPost>.Filter.Eq(p => p.Id, id) &
                         Builders<CassavaTubersPost>.Filter.Eq(p => p.SellerId, user.Id.ToString());

            await _posts.DeleteOneAsync(filter);
            TempData["SuccessMessage"] = "Post deleted successfully!";
            return RedirectToAction("MyPosts");
        }

        // GET: Index (all posts)
        public async Task<IActionResult> Index()
        {
            var posts = await _posts.Find(Builders<CassavaTubersPost>.Filter.Empty).ToListAsync();
            return View(posts);
        }
    }
}
