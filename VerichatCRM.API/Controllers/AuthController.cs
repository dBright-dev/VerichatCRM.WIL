using Microsoft.AspNetCore.Mvc;

namespace VerichatCRM.API.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
