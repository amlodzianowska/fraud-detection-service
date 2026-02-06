namespace FraudDetection.Core.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Location data
    public string? IpAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Device fingerprinting
    public string? DeviceId { get; set; }
    
    // Fraud detection results
    public int RiskScore { get; set; }
    public string Status { get; set; } = "pending";
}