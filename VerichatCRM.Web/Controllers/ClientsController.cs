using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Clients;
using VerichatCRM.Web.Models.Client;
using VerichatCRM.Web.Services;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IClientApiService _clients;

        public ClientsController(IClientApiService clients) => _clients = clients;

        private void SetHeader(string title, string subtitle)
        {
            ViewData["Title"] = title;
            ViewData["Subtitle"] = subtitle;
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";
        }

        public async Task<IActionResult> Index(string? search, string? sector)
        {
            SetHeader("Client Directory", "Search, filter, and drill into client accounts.");

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

            SetHeader(client.Name, $"{client.Sector} · Assigned to {client.AssignedRep}");
            return View(client);
        }

        [HttpGet]
        public IActionResult Create()
        {
            SetHeader("Add New Client", "Register a new client account.");
            return View(new CreateClientViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetHeader("Add New Client", "Please correct the highlighted fields.");
                return View(model);
            }

            var created = await _clients.CreateAsync(model);
            if (created == null)
            {
                ModelState.AddModelError("", "Failed to create client. Check that the API is running.");
                SetHeader("Add New Client", "Something went wrong.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"Client '{created.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
