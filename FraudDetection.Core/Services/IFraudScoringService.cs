using FraudDetection.Core.Entities;

namespace FraudDetection.Core.Services;

public interface IFraudScoringService
{
    Task<FraudAssessmentResult> AssessOrderAsync(Order order);
}