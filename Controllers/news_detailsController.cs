using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Ade_Farming.Controllers
{
        [Authorize]

    public class news_detailsController : Controller
    {
        public IActionResult new_detail()
        {
            return View();
        }
    }
}
