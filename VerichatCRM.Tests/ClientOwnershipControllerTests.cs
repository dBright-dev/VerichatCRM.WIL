using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.Controllers;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using Xunit;

namespace VerichatCRM.Tests;

public class ClientOwnershipControllerTests
{
    [Fact]
    public async Task ClientOrder_AlwaysUsesAuthenticatedClientId()
    {
        var service = new FakeOrderService();
        var controller = AsClient(new OrderController(service), "client-owned");
        await controller.CreateOrder(new CreateOrderRequest { ClientId = "someone-else", LineItems = [new() { SKU = "P1", Quantity = 1 }] });
        Assert.Equal("client-owned", service.LastRequest!.ClientId);
    }

    [Fact]
    public async Task ClientCannotReadAnotherClientsQuotation()
    {
        var service = new FakeQuotationService();
        var controller = AsClient(new QuotationController(service), "client-owned");
        var result = await controller.GetById("quote-1");
        Assert.IsType<ForbidResult>(result);
    }

    private static T AsClient<T>(T controller, string userId) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Role, UserRoles.ClientUser)
                ], "test"))
            }
        };
        return controller;
    }

    private sealed class FakeOrderService : IOrderService
    {
        public CreateOrderRequest? LastRequest { get; private set; }
        public Task<IEnumerable<OrderResponse>> GetOrdersAsync() => Task.FromResult<IEnumerable<OrderResponse>>([]);
        public Task<IEnumerable<OrderResponse>> GetOrdersByClientAsync(string clientId) => Task.FromResult<IEnumerable<OrderResponse>>([]);
        public Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request) { LastRequest = request; return Task.FromResult(new OrderResponse { OrderId = "o1", ClientId = request.ClientId }); }
        public Task<OrderResponse?> UpdateOrderStatusAsync(string orderId, string newStatus) => Task.FromResult<OrderResponse?>(null);
    }

    private sealed class FakeQuotationService : IQuotationService
    {
        public Task<IEnumerable<QuotationDto>> GetAllAsync() => Task.FromResult<IEnumerable<QuotationDto>>([]);
        public Task<IEnumerable<QuotationDto>> GetByClientAsync(string clientId) => Task.FromResult<IEnumerable<QuotationDto>>([]);
        public Task<QuotationDto?> GetByIdAsync(string quotationId) => Task.FromResult<QuotationDto?>(new QuotationDto { QuotationId = quotationId, ClientId = "another-client" });
        public Task<QuotationDto> CreateAsync(CreateQuotationRequest request) => Task.FromResult(new QuotationDto());
    }
}
