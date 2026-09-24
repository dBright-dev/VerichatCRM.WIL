using Microsoft.AspNetCore.Mvc;

namespace VerichatCRM.API.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
