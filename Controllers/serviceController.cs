using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Ade_Farming.Controllers
{
        [Authorize]

    public class serviceController : Controller
    {
        public IActionResult service()
        {
            return View();
        }
    }
}
