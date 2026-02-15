using FraudDetection.Core.Entities;
using FraudDetection.Core.Interfaces;
using FraudDetection.Core.Services;

namespace FraudDetection.Infrastructure.Services;

public class FraudScoringService : IFraudScoringService
{
    private readonly IUserBehaviorProfileRepository _profileRepository;
    private readonly IOrderRepository _orderRepository;

    public FraudScoringService(
        IUserBehaviorProfileRepository profileRepository,
        IOrderRepository orderRepository)
    {
        _profileRepository = profileRepository;
        _orderRepository = orderRepository;
    }

    public async Task<FraudAssessmentResult> AssessOrderAsync(Order order)
    {
        var riskFactors = new List<string>();
        var riskScore = 0;

        // Load or create user behavior profile
        var profile = await _profileRepository.GetByUserIdAsync(order.UserId);
    
        if (profile == null)
        {
            // New user - create profile
            profile = new UserBehaviorProfile
            {
                Id = Guid.NewGuid(),
                UserId = order.UserId,
                FirstOrderDate = DateTime.UtcNow,
                LastOrderDate = DateTime.UtcNow
            };
            riskScore += 20;
            riskFactors.Add("New account");
        }
        else
        {
            // Run fraud checks on existing user
            riskScore += CheckVelocity(profile, riskFactors);
            riskScore += CheckAmountDeviation(order, profile, riskFactors);
        }

        // Determine decision based on score
        var decision = riskScore switch
        {
            < 30 => FraudDecision.Approved,
            < 70 => FraudDecision.Review,
            _ => FraudDecision.Declined
        };

        return new FraudAssessmentResult
        {
            RiskScore = riskScore,
            Decision = decision,
            RiskFactors = riskFactors,
            AssessedAt = DateTime.UtcNow
        };
    }
    
    private int CheckVelocity(UserBehaviorProfile profile, List<string> riskFactors)
    {
        var velocityScore = 0;

        // Check 24-hour velocity
        if (profile.OrdersLast24Hours >= 5)
        {
            velocityScore += 30;
            riskFactors.Add($"High velocity: {profile.OrdersLast24Hours} orders in 24 hours");
        }
        else if (profile.OrdersLast24Hours >= 3)
        {
            velocityScore += 15;
            riskFactors.Add($"Moderate velocity: {profile.OrdersLast24Hours} orders in 24 hours");
        }

        // Check 7-day velocity
        if (profile.OrdersLast7Days >= 20)
        {
            velocityScore += 20;
            riskFactors.Add($"High weekly volume: {profile.OrdersLast7Days} orders in 7 days");
        }

        return velocityScore;
    }
    
    private int CheckAmountDeviation(Order order, UserBehaviorProfile profile, List<string> riskFactors)
    {
        var deviationScore = 0;

        // Skip check if user has no order history
        if (profile.TotalOrders == 0 || profile.AverageOrderAmount == 0)
        {
            return deviationScore;
        }

        // Calculate deviation from average
        var deviationMultiple = order.Amount / profile.AverageOrderAmount;

        if (deviationMultiple >= 5)
        {
            deviationScore += 40;
            riskFactors.Add($"Order amount ${order.Amount} is {deviationMultiple:F1}x higher than average ${profile.AverageOrderAmount}");
        }
        else if (deviationMultiple >= 3)
        {
            deviationScore += 25;
            riskFactors.Add($"Order amount ${order.Amount} is {deviationMultiple:F1}x higher than average ${profile.AverageOrderAmount}");
        }

        return deviationScore;
    }
}