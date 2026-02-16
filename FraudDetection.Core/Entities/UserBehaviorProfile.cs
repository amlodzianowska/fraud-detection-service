namespace FraudDetection.Core.Entities;

public class UserBehaviorProfile
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    // Order patterns
    public int TotalOrders { get; set; }
    public decimal AverageOrderAmount { get; set; }
    public decimal LifetimeSpend { get; set; }
    
    // Geographic patterns
    public string? PrimaryLocation { get; set; }
    public int UniqueLocationsCount { get; set; }
    
    // Device tracking
    public int UniqueDevicesCount { get; set; }
    
    // Risk indicators
    public int ChargebackCount { get; set; }
    public int RefundRequestCount { get; set; }
    
    // Timestamps
    public DateTime FirstOrderDate { get; set; }
    public DateTime LastOrderDate { get; set; }
    public DateTime LastUpdated { get; set; }
}