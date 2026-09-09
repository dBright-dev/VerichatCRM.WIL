using Microsoft.AspNetCore.Mvc;

namespace VerichatCRM.API.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
