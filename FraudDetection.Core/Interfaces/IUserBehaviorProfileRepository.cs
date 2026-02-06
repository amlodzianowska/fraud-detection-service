using FraudDetection.Core.Entities;

namespace FraudDetection.Core.Interfaces;

public interface IUserBehaviorProfileRepository
{
    Task<UserBehaviorProfile?> GetByUserIdAsync(string userId);
    Task<UserBehaviorProfile> CreateAsync(UserBehaviorProfile profile);
    Task<UserBehaviorProfile> UpdateAsync(UserBehaviorProfile profile);
}