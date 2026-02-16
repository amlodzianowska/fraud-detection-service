using FraudDetection.Api.Models;
using FraudDetection.Core.Entities;
using FraudDetection.Core.Interfaces;
using FraudDetection.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FraudDetection.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IFraudScoringService _fraudScoringService;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserBehaviorProfileRepository _profileRepository;

    public OrdersController(
        IFraudScoringService fraudScoringService,
        IOrderRepository orderRepository,
        IUserBehaviorProfileRepository profileRepository)
    {
        _fraudScoringService = fraudScoringService;
        _orderRepository = orderRepository;
        _profileRepository = profileRepository;
    }
    
    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderRequestDto request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = request.Currency,
            CreatedAt = DateTime.UtcNow,
            IpAddress = request.IpAddress,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            DeviceId = request.DeviceId
        };
        
        var assessment = await _fraudScoringService.AssessOrderAsync(order);
        
        order.RiskScore = assessment.RiskScore;
        order.Status = assessment.Decision switch
        {
            FraudDecision.Approved => OrderStatus.Approved,
            FraudDecision.Review => OrderStatus.UnderReview,
            FraudDecision.Declined => OrderStatus.Declined,
            _ => OrderStatus.Pending
        };
        
        await _orderRepository.CreateAsync(order);
        
        var response = new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Amount = order.Amount,
            Currency = order.Currency,
            CreatedAt = order.CreatedAt,
            IpAddress = order.IpAddress,
            Latitude = order.Latitude,
            Longitude = order.Longitude,
            DeviceId = order.DeviceId,
            RiskScore = order.RiskScore,
            Status = order.Status,
            FraudDecision = assessment.Decision,
            RiskFactors = assessment.RiskFactors
        };

        return Ok(response);
    }
    
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<OrderResponseDto>>> GetUserOrders(string userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
    
        var response = orders.Select(order => new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Amount = order.Amount,
            Currency = order.Currency,
            CreatedAt = order.CreatedAt,
            IpAddress = order.IpAddress,
            Latitude = order.Latitude,
            Longitude = order.Longitude,
            DeviceId = order.DeviceId,
            RiskScore = order.RiskScore,
            Status = order.Status,
            FraudDecision = order.Status switch
            {
                OrderStatus.Approved => FraudDecision.Approved,
                OrderStatus.Declined => FraudDecision.Declined,
                OrderStatus.UnderReview => FraudDecision.Review,
                _ => FraudDecision.Review
            },
            RiskFactors = new List<string>()
        }).ToList();
    
        return Ok(response);
    }
    
    [HttpGet("user/velocity/{userId}")]
    public async Task<ActionResult> GetUserOrderVelocity(string userId)
    {
        var last24Hours = await _orderRepository.GetRecentByUserIdAsync(
            userId, DateTime.UtcNow.AddHours(-24));
        var last7Days = await _orderRepository.GetRecentByUserIdAsync(
            userId, DateTime.UtcNow.AddDays(-7));
        var last30Days = await _orderRepository.GetRecentByUserIdAsync(
            userId, DateTime.UtcNow.AddDays(-30));

        var response = new
        {
            OrdersLast24Hours = last24Hours.Count,
            OrdersLast7Days = last7Days.Count,
            OrdersLast30Days = last30Days.Count
        };

        return Ok(response);
    }
}