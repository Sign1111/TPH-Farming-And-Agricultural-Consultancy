using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Ade_Farming.Controllers
{
        [Authorize]

    public class contactController : Controller
    {
        public IActionResult contact()
        {
            return View();
        }
    }
}
