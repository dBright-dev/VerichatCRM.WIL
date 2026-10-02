using System.Collections.Generic;
using System.Threading.Tasks;
using VerichatCRM.Web.Models.Support;

namespace VerichatCRM.Web.Services.Interfaces;

public interface ISupportApiService
{
    Task<List<TicketDto>> GetTicketsAsync();
    Task<TicketDto?> GetTicketAsync(string ticketId);
    Task<TicketDto?> CreateTicketAsync(CreateTicketDto dto);
    Task<TicketDto?> UpdateTicketStatusAsync(string ticketId, string newStatus, string? notes);
    Task<List<AssetDto>> GetAssetsAsync();
    Task<List<DeliveryDto>> GetDeliveriesAsync();
}