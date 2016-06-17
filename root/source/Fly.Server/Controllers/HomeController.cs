using Microsoft.AspNetCore.Mvc;

namespace Fly.Server.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet, Route("~/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
