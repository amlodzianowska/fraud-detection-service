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
        var isNewUser = false;

        if (profile == null)
        {
            isNewUser = true;
            profile = new UserBehaviorProfile
            {
                Id = Guid.NewGuid(),
                UserId = order.UserId,
                FirstOrderDate = DateTime.UtcNow,
                LastOrderDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
            riskScore += 20;
            riskFactors.Add("New account");
        }
        else
        {
            // Run fraud checks on existing user
            riskScore += await CheckVelocity(order, riskFactors);
            riskScore += CheckAmountDeviation(order, profile, riskFactors);
        }

        // Determine decision based on score
        var decision = riskScore switch
        {
            < 30 => FraudDecision.Approved,
            < 70 => FraudDecision.Review,
            _ => FraudDecision.Declined
        };

        // Update profile counters with this order's data
        UpdateProfileCounters(profile, order.Amount);

        // Persist the profile
        if (isNewUser)
        {
            await _profileRepository.CreateAsync(profile);
        }
        else
        {
            await _profileRepository.UpdateAsync(profile);
        }

        return new FraudAssessmentResult
        {
            RiskScore = riskScore,
            Decision = decision,
            RiskFactors = riskFactors,
            AssessedAt = DateTime.UtcNow
        };
    }

    private void UpdateProfileCounters(UserBehaviorProfile profile, decimal orderAmount)
    {
        // Recalculate average:
        profile.LifetimeSpend += orderAmount;
        profile.TotalOrders += 1;
        profile.AverageOrderAmount = profile.LifetimeSpend / profile.TotalOrders;

        // Update timestamps
        profile.LastOrderDate = DateTime.UtcNow;
        profile.LastUpdated = DateTime.UtcNow;
    }

    private async Task<int> CheckVelocity(Order order, List<string> riskFactors)
    {
        var velocityScore = 0;
        var last24Hours = await _orderRepository.GetRecentByUserIdAsync(
            order.UserId, DateTime.UtcNow.AddHours(-24));
        var last7Days = await _orderRepository.GetRecentByUserIdAsync(
            order.UserId, DateTime.UtcNow.AddDays(-7));

        if (last24Hours.Count >= 5)
        {
            velocityScore += 30;
            riskFactors.Add($"High velocity: {last24Hours.Count} orders in 24 hours");
        }
        else if (last24Hours.Count >= 3)
        {
            velocityScore += 15;
            riskFactors.Add($"Moderate velocity: {last24Hours.Count} orders in 24 hours");
        }

        if (last7Days.Count >= 20)
        {
            velocityScore += 20;
            riskFactors.Add($"High weekly volume: {last7Days.Count} orders in 7 days");
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
            riskFactors.Add($"Order amount ${order.Amount} is {deviationMultiple:F}x higher than average ${profile.AverageOrderAmount}");
        }
        else if (deviationMultiple >= 3)
        {
            deviationScore += 25;
            riskFactors.Add($"Order amount ${order.Amount} is {deviationMultiple:F1}x higher than average ${profile.AverageOrderAmount}");
        }

        return deviationScore;
    }
}