using FraudDetection.Core.Entities;

namespace FraudDetection.Api.Models;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; }
    
    // Location data
    public string? IpAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DeviceId { get; set; }
    
    // Fraud assessment results
    public int RiskScore { get; set; }
    public OrderStatus Status { get; set; }
    public FraudDecision FraudDecision { get; set; }
    public List<RiskFactor> RiskFactors { get; set; } = new();
}