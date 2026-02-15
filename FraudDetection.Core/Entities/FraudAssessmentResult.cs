namespace FraudDetection.Core.Entities;

public class FraudAssessmentResult
{
    public int RiskScore { get; set; }
    
    public FraudDecision Decision { get; set; }
    
    public List<string> RiskFactors { get; set; } = new();
    
    public DateTime AssessedAt { get; set; }
}