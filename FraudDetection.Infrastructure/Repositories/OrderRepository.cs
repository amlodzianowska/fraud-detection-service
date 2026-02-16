using FraudDetection.Core.Entities;
using FraudDetection.Core.Interfaces;
using FraudDetection.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FraudDetection.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> GetByIdAsync(Guid orderId)
    {
        return await _dbContext.Orders
            .Include(o => o.RiskFactors)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Order>> GetByUserIdAsync(string userId)
    {
        return await _dbContext.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetRecentByUserIdAsync(string userId, DateTime since)
    {
        return await _dbContext.Orders
            .Where(o => o.UserId == userId && o.CreatedAt >= since)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}