using FraudDetection.Core.Entities;
using FraudDetection.Core.Interfaces;
using FraudDetection.Infrastructure.Services;
using NSubstitute;
using Xunit;

namespace FraudDetection.UnitTests;

public class FraudScoringServiceTests
{
    [Fact]
    public async Task AssessOrderAsync_NewUser_ReturnsScore20AndApproved()
    {
        // 1. Set up
        var profileRepo = Substitute.For<IUserBehaviorProfileRepository>();
        var orderRepo = Substitute.For<IOrderRepository>();

        var service = new FraudScoringService(profileRepo, orderRepo);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = "new-user-123",
            Amount = 50m,
            CreatedAt = DateTime.UtcNow
        };

        // 2. Run
        var result = await service.AssessOrderAsync(order);

        // 3. Check
        Assert.Equal(20, result.RiskScore);
        Assert.Equal(FraudDecision.Approved, result.Decision);
        Assert.Single(result.RiskFactors);
        Assert.Equal(RiskFactorType.AccountAge, result.RiskFactors[0].Type);
    }
}