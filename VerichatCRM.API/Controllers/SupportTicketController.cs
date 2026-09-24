using Microsoft.AspNetCore.Mvc;

namespace VerichatCRM.API.Controllers
{
    public class SupportTicketController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
