using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace Ade_Farming.Controllers
{
    [Authorize]
    public class AboutController : Controller
    {
        public IActionResult about()
        {
            return View();
        }
    }
}
