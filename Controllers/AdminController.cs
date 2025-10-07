using Ade_Farming.Hubs;
using Ade_Farming.Models;
using Ade_Farming.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ade_Farming.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMongoDatabase _database;
		private readonly IHubContext<AdminHub> _hubContext;




		public AdminController(UserManager<ApplicationUser> userManager, IMongoDatabase database, IHubContext<AdminHub> hubContext)
		{
			_userManager = userManager;
			_hubContext = hubContext;
			_database = database;

        }

        // Dashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var totalUsers = users.Count;

            // Map to ViewModel
            var model = new List<AdminUserViewModel>();
            foreach (var user in users)
            {
                model.Add(new AdminUserViewModel
                {
                    Id = user.Id.ToString(),
                    FullName = user.FullName,
                    Email = user.Email,
                    IsAdmin = await _userManager.IsInRoleAsync(user, "Admin")
                });
            }

            // --- Count posts logic remains the same ---
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            var startOfYear = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfYear = startOfYear.AddYears(1);

            var bananaPosts = _database.GetCollection<UserPost>("BananaSuckerPosts");
            var cassavaStemPosts = _database.GetCollection<UserPost>("CassavaStemPosts");
            var cassavaTubersPosts = _database.GetCollection<UserPost>("CassavaTubersPosts");
            var cocoaPalmPosts = _database.GetCollection<UserPost>("CocoaPalmSeedlingPosts");
            var oilPalmPosts = _database.GetCollection<UserPost>("OilPalmSeedlingPosts");
            var plantainPosts = _database.GetCollection<UserPost>("PlantainSuckerPosts");

            var postCollections = new IMongoCollection<UserPost>[]
            {
        bananaPosts,
        cassavaStemPosts,
        cassavaTubersPosts,
        cocoaPalmPosts,
        oilPalmPosts,
        plantainPosts
            };

            int totalPostsToday = 0;
            int totalPostsThisWeek = 0;
            int totalPostsThisYear = 0;

            foreach (var collection in postCollections)
            {
                totalPostsToday += (int)await collection.CountDocumentsAsync(
                    Builders<UserPost>.Filter.And(
                        Builders<UserPost>.Filter.Gte(p => p.CreatedAt, today),
                        Builders<UserPost>.Filter.Lt(p => p.CreatedAt, tomorrow)
                    ));

                totalPostsThisWeek += (int)await collection.CountDocumentsAsync(
                    Builders<UserPost>.Filter.And(
                        Builders<UserPost>.Filter.Gte(p => p.CreatedAt, startOfWeek),
                        Builders<UserPost>.Filter.Lt(p => p.CreatedAt, endOfWeek)
                    ));

                totalPostsThisYear += (int)await collection.CountDocumentsAsync(
                    Builders<UserPost>.Filter.And(
                        Builders<UserPost>.Filter.Gte(p => p.CreatedAt, startOfYear),
                        Builders<UserPost>.Filter.Lt(p => p.CreatedAt, endOfYear)
                    ));
            }

            ViewBag.TotalUsers = totalUsers;
            ViewBag.PostsToday = totalPostsToday;
            ViewBag.PostsWeek = totalPostsThisWeek;
            ViewBag.PostsYear = totalPostsThisYear;

            return View(model); // now passing List<AdminUserViewModel>
        }


		// Assign Admin Role
		[HttpPost]
		public async Task<IActionResult> AssignAdmin(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				return BadRequest();

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return NotFound();

			if (!await _userManager.IsInRoleAsync(user, "Admin"))
			{
				await _userManager.AddToRoleAsync(user, "Admin");

				// Optional: Notify the user that they now have admin privileges
				// Convert Guid to string for SignalR
				await _hubContext.Clients.User(user.Id.ToString())
					.SendAsync("ReceiveAdminAssigned");
			}

			return RedirectToAction("Index");
		}


		// Remove Admin Role
		[HttpPost]
public async Task<IActionResult> RemoveAdmin(string userId)
{
    if (string.IsNullOrEmpty(userId))
        return BadRequest();

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
        return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");

                // Force logout via SignalR
                await _hubContext.Clients.User(user.Id.ToString())
                    .SendAsync("ReceiveForceLogout");
            }



            return RedirectToAction("Index");
}

    }
}
