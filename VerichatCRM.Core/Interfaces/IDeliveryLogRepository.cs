using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Core.Interfaces
{
    public interface IDeliveryLogRepository
    {
        Task<IEnumerable<DeliveryLog>> GetAllAsync();
        Task<IEnumerable<DeliveryLog>> GetByClientAsync(string clientId);
        Task<DeliveryLog?> GetByTrackingAsync(string trackingNumber);
        Task<DeliveryLog> AddAsync(DeliveryLog log);
        Task UpdateAsync(DeliveryLog log);
    }
}
