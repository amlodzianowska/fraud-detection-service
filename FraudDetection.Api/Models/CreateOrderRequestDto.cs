namespace FraudDetection.Api.Models;

public class CreateOrderRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Location Data
    public string? IpAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Device fingerprint
    public string? DeviceId { get; set; }
}