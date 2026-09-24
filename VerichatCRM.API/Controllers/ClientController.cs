using Microsoft.AspNetCore.Mvc;

namespace VerichatCRM.API.Controllers
{
    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
