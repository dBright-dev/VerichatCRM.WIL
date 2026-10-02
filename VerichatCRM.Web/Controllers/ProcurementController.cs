using Microsoft.AspNetCore.Mvc;

namespace VerichatCRM.Web.Controllers
{
    public class ProcurementController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Procurement Dashboard";
            ViewData["Subtitle"] = "Reorder consumables, track deliveries, monitor stock.";
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Procurement Manager";
            return View();
        }

        public IActionResult Warehouse() => View();
    }
}
