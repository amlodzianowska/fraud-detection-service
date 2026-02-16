using FraudDetection.Core.Entities;

namespace FraudDetection.Core.Interfaces;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);
    Task<Order?> GetByIdAsync(Guid orderId);
    Task<List<Order>> GetByUserIdAsync(string userId);
    Task<List<Order>> GetRecentByUserIdAsync(string userId, DateTime since);
    Task<Order> UpdateAsync(Order order);
}