using FraudDetection.Core.Entities;
using FraudDetection.Core.Interfaces;
using FraudDetection.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FraudDetection.Infrastructure.Repositories;

public class UserBehaviorProfileRepository : IUserBehaviorProfileRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserBehaviorProfileRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserBehaviorProfile?> GetByUserIdAsync(string userId)
    {
        return await _dbContext.UserBehaviorProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<UserBehaviorProfile> CreateAsync(UserBehaviorProfile profile)
    {
        _dbContext.UserBehaviorProfiles.Add(profile);
        await _dbContext.SaveChangesAsync();
        return profile;
    }

    public async Task<UserBehaviorProfile> UpdateAsync(UserBehaviorProfile profile)
    {
        _dbContext.UserBehaviorProfiles.Update(profile);
        await _dbContext.SaveChangesAsync();
        return profile;
    }
}