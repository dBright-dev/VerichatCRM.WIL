using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Controllers
{
    /// <summary>Mobile account: profile, settings, quick reorder (1-tap catalogue).</summary>
    [ApiController]
    [Route("api/mobile")]
    [Authorize]
    public class MobileAccountController : ControllerBase
    {
        private readonly IMobileService _mobile;
        private readonly IAuthService _auth;
        private readonly AppDbContext _db;

        public MobileAccountController(IMobileService mobile, IAuthService auth, AppDbContext db)
        {
            _mobile = mobile;
            _auth = auth;
            _db = db;
        }

        private string UserId =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("No user id claim.");

        /// <summary>Client self-registration → "Client User" role, JWT included.</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] ClientRegisterRequest request)
        {
            // Registration spans Users, MobileProfiles and Clients, all in the
            // same CRM database. Commit them together so a failed profile/client
            // insert cannot leave a half-created account behind.
            var strategy = _db.Database.CreateExecutionStrategy();
            var result = await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();
                var registration = await _auth.RegisterAsync(new RegisterRequest
                {
                    DisplayName = request.DisplayName,
                    Email = request.Email,
                    Password = request.Password,
                    Role = Core.Entities.UserRoles.ClientUser
                });

                if (!registration.Success || registration.User == null)
                {
                    await transaction.RollbackAsync();
                    return registration;
                }

                if (!string.IsNullOrWhiteSpace(request.CompanyName))
                {
                    await _mobile.UpdateProfileAsync(
                        registration.User.UserId,
                        _ => { },
                        p => p.Company = request.CompanyName!.Trim());
                }

                await _mobile.EnsureClientAccountAsync(registration.User.UserId);
                await transaction.CommitAsync();
                return registration;
            });

            if (!result.Success || result.User == null)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> Profile() =>
            Ok(await _mobile.GetProfileAsync(UserId));

        [HttpPatch("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var profile = await _mobile.UpdateProfileAsync(UserId,
                user =>
                {
                    if (!string.IsNullOrWhiteSpace(request.DisplayName))
                        user.DisplayName = request.DisplayName.Trim();
                },
                p =>
                {
                    if (request.Phone != null) p.Phone = request.Phone;
                    if (request.Company != null) p.Company = request.Company;
                    if (request.VatNumber != null) p.VatNumber = request.VatNumber;
                    if (request.AddressName != null) p.AddressName = request.AddressName;
                    if (request.Street != null) p.Street = request.Street;
                    if (request.City != null) p.City = request.City;
                    if (request.Country != null) p.Country = request.Country;
                });
            return Ok(profile);
        }

        [HttpGet("settings")]
        public async Task<IActionResult> Settings() =>
            Ok(await _mobile.GetSettingsAsync(UserId));

        [HttpPatch("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request) =>
            Ok(await _mobile.UpdateSettingsAsync(UserId,
                request.PushNotifications, request.BiometricLogin, request.DarkMode));

        /// <summary>1-tap reorder of a recurring consumable → pre-filled order.</summary>
        [HttpPost("reorder/{sku}")]
        public async Task<IActionResult> QuickReorder(string sku, [FromServices] IOrderService orders)
        {
            // Self-heal: older mobile accounts may pre-date the Client row.
            await _mobile.EnsureClientAccountAsync(UserId);

            try
            {
                var order = await orders.CreateOrderAsync(new CreateOrderRequest
                {
                    ClientId = UserId,
                    LineItems = new List<OrderLineItemRequest> { new() { SKU = sku, Quantity = 1 } }
                });
                return Ok(new { orderId = order.OrderId, total = order.TotalAmount, status = order.Status });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, [FromServices] IOrderService orders)
        {
            await _mobile.EnsureClientAccountAsync(UserId);
            request.ClientId = UserId;
            try { return Ok(await orders.CreateOrderAsync(request)); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromServices] IOrderService orders)
        {
            // A read must not manufacture a CRM client record for a staff user
            // who opens the mobile app. The order service returns only rows
            // matching this authenticated user ID.
            return Ok(await orders.GetOrdersByClientAsync(UserId));
        }

    }
}
