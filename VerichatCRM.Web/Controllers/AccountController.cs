using Microsoft.AspNetCore.Mvc;

namespace VerichatCRM.Web.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
