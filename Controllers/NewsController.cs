using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Ade_Farming.Controllers
{
        [Authorize]

    public class NewsController : Controller
    {
        public IActionResult news()
        {
            return View();
        }
    }
}
