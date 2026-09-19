using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Quotation;

namespace VerichatCRM.Web.Controllers
{
    public class QuotationController : Controller
    {
        public IActionResult Index(int? clientId)
        {
            ViewData["Title"] = "Quotation Builder";
            ViewData["Subtitle"] = "Create and manage client quotations.";
            ViewData["CurrentRole"] = "Sales Representative";

            var (name, sector) = GetMockClientSummary(clientId);

            var model = new QuotationViewModel
            {
                ClientId = clientId ?? 0,
                ClientName = name,
                ClientSector = sector,
                QuoteReference = $"QUO-{DateTime.Today:yyyy}-{Random.Shared.Next(100, 999)}",
                LineItems = new List<QuotationLineItemViewModel>
            {
                new() { RowId = 1, ProductName = "First Response Defibrillator", Sku = "MD-FRD-014", Quantity = 2, UnitPrice = 18400m },
                new() { RowId = 2, ProductName = "Surgical Trolley (Stainless)", Sku = "MD-STR-002", Quantity = 4, UnitPrice = 3200m },
            }
            };

            return View(model);
        }

        private static (string Name, string Sector) GetMockClientSummary(int? clientId)
        {
            return clientId switch
            {
                1 => ("Anglo American", "Mining"),
                2 => ("Nexam Holdings", "Manufacturing"),
                3 => ("Siemens Healthineers", "Clinical"),
                4 => ("Landfill Solutions SA", "Waste Management"),
                _ => ("Select a client", "")
            };
        }
    }
}
