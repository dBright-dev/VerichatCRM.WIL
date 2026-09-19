using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Clients;

namespace VerichatCRM.Web.Controllers
{
    public class ClientsController : Controller
    {
        public IActionResult Index(string? search, string? sector)
        {
            ViewData["Title"] = "Client Directory";
            ViewData["Subtitle"] = "Search, filter, and drill into client accounts.";
            ViewData["CurrentRole"] = "Sales Representative";

            var clients = GetMockClients();

            if (!string.IsNullOrWhiteSpace(search))
            {
                clients = clients
                    .Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || c.ContactPerson.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(sector) && sector != "All")
            {
                clients = clients
                    .Where(c => c.Sector.Equals(sector, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var model = new ClientDirectoryViewModel
            {
                Clients = clients,
                SearchTerm = search,
                SectorFilter = sector,
                AvailableSectors = GetMockClients()
                    .Select(c => c.Sector)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList()
            };

            return View(model);
        }

        public IActionResult Profile(int id)
        {
            var client = GetMockClients().FirstOrDefault(c => c.Id == id);

            if (client is null)
            {
                return NotFound();
            }

            ViewData["Title"] = client.Name;
            ViewData["Subtitle"] = $"{client.Sector} · Assigned to {client.AssignedRep}";
            ViewData["CurrentRole"] = "Sales Representative";

            return View(client);
        }

        private static List<ClientViewModel> GetMockClients()
        {
            return new List<ClientViewModel>
        {
            new()
            {
                Id = 1, Name = "Anglo American", Sector = "Mining",
                ContactPerson = "Naledi Mokoena", ContactEmail = "n.mokoena@angloamerican.example",
                ContactPhone = "+27 11 555 0142", Status = "Active", AssignedRep = "Thabo N.",
                TotalDealsValue = 3200000m, LastContactDate = DateTime.Today.AddDays(-2)
            },
            new()
            {
                Id = 2, Name = "Nexam Holdings", Sector = "Manufacturing",
                ContactPerson = "Ryan Fisher", ContactEmail = "r.fisher@nexam.example",
                ContactPhone = "+27 21 555 0198", Status = "Prospect", AssignedRep = "Mamoyo M.",
                TotalDealsValue = 1200000m, LastContactDate = DateTime.Today.AddDays(-6)
            },
            new()
            {
                Id = 3, Name = "Siemens Healthineers", Sector = "Clinical",
                ContactPerson = "Anele Dube", ContactEmail = "a.dube@siemens-health.example",
                ContactPhone = "+27 12 555 0177", Status = "Active", AssignedRep = "Thabo N.",
                TotalDealsValue = 4550000m, LastContactDate = DateTime.Today.AddDays(-1)
            },
            new()
            {
                Id = 4, Name = "Landfill Solutions SA", Sector = "Waste Management",
                ContactPerson = "Karabo Sithole", ContactEmail = "k.sithole@landfillsa.example",
                ContactPhone = "+27 31 555 0163", Status = "Inactive", AssignedRep = "Mamoyo M.",
                TotalDealsValue = 875000m, LastContactDate = DateTime.Today.AddDays(-30)
            },
        };
        }
    }
}
