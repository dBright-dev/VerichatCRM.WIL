using Microsoft.AspNetCore.Mvc;

namespace VerichatCRM.Web.Controllers
{
    public class QuotationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
