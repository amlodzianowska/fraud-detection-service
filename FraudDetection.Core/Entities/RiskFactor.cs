namespace FraudDetection.Core.Entities;

public class RiskFactor
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    
    public RiskFactorType Type { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
}