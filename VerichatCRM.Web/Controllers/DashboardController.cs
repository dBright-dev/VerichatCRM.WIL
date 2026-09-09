using Microsoft.AspNetCore.Mvc;

namespace VerichatCRM.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
