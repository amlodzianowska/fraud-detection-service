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
        order.RiskFactors = assessment.RiskFactors;
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
            RiskFactors = new List<RiskFactor>()
        }).ToList();
    
        return Ok(response);
    }
    
    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderResponseDto>> GetOrderById(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
            return NotFound();

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
            FraudDecision = order.Status switch
            {
                OrderStatus.Approved => FraudDecision.Approved,
                OrderStatus.Declined => FraudDecision.Declined,
                OrderStatus.UnderReview => FraudDecision.Review,
                _ => FraudDecision.Review
            },
            RiskFactors = new List<RiskFactor>()
        };

        return Ok(response);
    }
    
    [HttpPatch("{orderId:guid}/review")]
    public async Task<ActionResult<OrderResponseDto>> ReviewOrder(Guid orderId, [FromBody] ReviewOrderRequestDto request)
    {
        if (request.Decision != FraudDecision.Approved && request.Decision != FraudDecision.Declined)
        {
            return BadRequest("Decision must be either Approved or Declined.");
        }

        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            return NotFound();
        }

        if (order.Status != OrderStatus.UnderReview)
        {
            return BadRequest($"Order cannot be reviewed. Current status: {order.Status}");
        }

        order.Status = request.Decision == FraudDecision.Approved
            ? OrderStatus.Approved
            : OrderStatus.Declined;

        await _orderRepository.UpdateAsync(order);

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
            FraudDecision = request.Decision,
            RiskFactors = order.RiskFactors
        };

        return Ok(response);
    }
}