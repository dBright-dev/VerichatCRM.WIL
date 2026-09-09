using Microsoft.AspNetCore.Mvc;

namespace VerichatCRM.API.Controllers
{
    public class AssetController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
