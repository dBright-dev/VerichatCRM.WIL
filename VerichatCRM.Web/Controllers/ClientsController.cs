using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Clients;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models.Client;
using VerichatCRM.Web.Services;
using VerichatCRM.Web.Services.Interfaces;
using VerichatCRM.Web.ViewModels;

namespace VerichatCRM.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IClientApiService _clients;
        private readonly IApiClient _api;
        private readonly IAdminApiService _admin;
        private readonly IClientFieldApiService _field;

        public ClientsController(IClientApiService clients, IApiClient api,
                                 IAdminApiService admin, IClientFieldApiService field)
        {
            _clients = clients;
            _api = api;
            _admin = admin;
            _field = field;
        }

        private void SetHeader(string title, string subtitle)
        {
            ViewData["Title"] = title;
            ViewData["Subtitle"] = subtitle;
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";
        }

        [RequirePermission(Perm.ClientDirectory)]
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

        [RequirePermission(Perm.ClientProfile)]
        public async Task<IActionResult> Profile(string id)
        {
            var client = await _clients.GetClientAsync(id);
            if (client is null) return NotFound();

            SetHeader(client.Name, $"{client.Sector} · Assigned to {client.AssignedRep}");
            var orders = await _api.GetAsync<List<ClientOrderRow>>($"/api/order/client/{Uri.EscapeDataString(id)}") ?? new();
            var quotations = await _api.GetAsync<List<ClientQuotationRow>>($"/api/quotation/client/{Uri.EscapeDataString(id)}") ?? new();

            // Assignment card is admin-only; skip the extra API calls for everyone else.
            var role = HttpContext.Session.GetString("UserRole") ?? "";
            var canAssign = Perm.Has(role, Perm.ClientAssign);
            ViewData["CanAssign"] = canAssign;

            if (canAssign)
            {
                var reps = new List<UserListItem>();
                try
                {
                    reps = (await _admin.GetUsersAsync())
                        .Where(u => u.IsActive && u.Role == Roles.SalesRepresentative)
                        .OrderBy(u => u.DisplayName)
                        .ToList();
                }
                catch
                {
                    // Show the card with an empty list rather than failing the whole profile.
                }

                ViewData["Reps"] = reps;
                ViewData["AssignmentHistory"] = await _field.GetAssignmentHistoryAsync(id);
            }

            return View(new ClientProfileViewModel { Client = client, Orders = orders, Quotations = quotations });
        }

        [RequirePermission(Perm.ClientAssign)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(string id, string salesRepId)
        {
            if (string.IsNullOrWhiteSpace(salesRepId))
            {
                TempData["ErrorMessage"] = "Choose a sales representative first.";
                return RedirectToAction(nameof(Profile), new { id });
            }

            var ok = await _field.AssignAsync(id, salesRepId);
            if (ok) TempData["SuccessMessage"] = "Client reassigned.";
            else TempData["ErrorMessage"] = "Couldn't reassign this client. Please try again.";
            return RedirectToAction(nameof(Profile), new { id });
        }

        [RequirePermission(Perm.ClientCreate)]
        [HttpGet]
        public IActionResult Create()
        {
            SetHeader("Add New Client", "Register a new client account.");
            return View(new CreateClientViewModel());
        }

        [RequirePermission(Perm.ClientCreate)]
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