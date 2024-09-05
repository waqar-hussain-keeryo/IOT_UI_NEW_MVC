using Microsoft.AspNetCore.Mvc;

namespace IOT_UI.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            return View();
        }
    }
}
