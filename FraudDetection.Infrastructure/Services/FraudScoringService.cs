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
        var riskFactors = new List<RiskFactor>();
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
            riskFactors.Add(new RiskFactor
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Type = RiskFactorType.AccountAge,
                Description = "New account",
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            var (velocityScore, velocityFactors) = await CheckVelocity(order);
            riskScore += velocityScore;
            riskFactors.AddRange(velocityFactors);

            var (deviationScore, deviationFactors) = CheckAmountDeviation(order, profile);
            riskScore += deviationScore;
            riskFactors.AddRange(deviationFactors);
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

    private async Task<(int score, List<RiskFactor> factors)> CheckVelocity(Order order)
    {
        var score = 0;
        var factors = new List<RiskFactor>();

        var last24Hours = await _orderRepository.GetRecentByUserIdAsync(
            order.UserId, DateTime.UtcNow.AddHours(-24));
        var last7Days = await _orderRepository.GetRecentByUserIdAsync(
            order.UserId, DateTime.UtcNow.AddDays(-7));

        if (last24Hours.Count >= 5)
        {
            score += 30;
            factors.Add(new RiskFactor
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Type = RiskFactorType.Velocity,
                Description = $"High velocity: {last24Hours.Count} orders in 24 hours",
                CreatedAt = DateTime.UtcNow
            });
        }
        else if (last24Hours.Count >= 3)
        {
            score += 15;
            factors.Add(new RiskFactor
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Type = RiskFactorType.Velocity,
                Description = $"Moderate velocity: {last24Hours.Count} orders in 24 hours",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (last7Days.Count >= 20)
        {
            score += 20;
            factors.Add(new RiskFactor
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Type = RiskFactorType.Velocity,
                Description = $"High weekly volume: {last7Days.Count} orders in 7 days",
                CreatedAt = DateTime.UtcNow
            });
        }

        return (score, factors);
    }

    private (int score, List<RiskFactor> factors) CheckAmountDeviation(Order order, UserBehaviorProfile profile)
    {
        var score = 0;
        var factors = new List<RiskFactor>();

        if (profile.TotalOrders == 0 || profile.AverageOrderAmount == 0)
        {
            return (score, factors);
        }

        var deviationMultiple = order.Amount / profile.AverageOrderAmount;

        if (deviationMultiple >= 5)
        {
            score += 40;
            factors.Add(new RiskFactor
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Type = RiskFactorType.AmountDeviation,
                Description = $"Order amount ${order.Amount} is {deviationMultiple:F1}x higher than average ${profile.AverageOrderAmount}",
                CreatedAt = DateTime.UtcNow
            });
        }
        else if (deviationMultiple >= 3)
        {
            score += 25;
            factors.Add(new RiskFactor
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Type = RiskFactorType.AmountDeviation,
                Description = $"Order amount ${order.Amount} is {deviationMultiple:F1}x higher than average ${profile.AverageOrderAmount}",
                CreatedAt = DateTime.UtcNow
            });
        }

        return (score, factors);
    }
}