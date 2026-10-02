using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Clients;
using VerichatCRM.Web.Services;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IClientApiService _clients;

        public ClientsController(IClientApiService clients) => _clients = clients;

        public async Task<IActionResult> Index(string? search, string? sector)
        {
            ViewData["Title"] = "Client Directory";
            ViewData["Subtitle"] = "Search, filter, and drill into client accounts.";
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";

            var clients = await _clients.GetClientsAsync(search, sector);

            var model = new ClientDirectoryViewModel
            {
                Clients = clients,
                SearchTerm = search,
                SectorFilter = sector,
                AvailableSectors = clients.Select(c => c.Sector).Distinct().OrderBy(s => s).ToList()
            };
            return View(model);
        }

        public async Task<IActionResult> Profile(string id)
        {
            var client = await _clients.GetClientAsync(id);
            if (client is null) return NotFound();

            ViewData["Title"] = client.Name;
            ViewData["Subtitle"] = $"{client.Sector} · Assigned to {client.AssignedRep}";
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";

            return View(client);
        }
    }
}