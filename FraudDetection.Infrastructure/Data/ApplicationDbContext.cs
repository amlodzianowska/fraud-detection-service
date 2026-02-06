using FraudDetection.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FraudDetection.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<UserBehaviorProfile> UserBehaviorProfiles => Set<UserBehaviorProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });
        
        modelBuilder.Entity<UserBehaviorProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.AverageOrderAmount).HasPrecision(18, 2);
            entity.Property(e => e.LifetimeSpend).HasPrecision(18, 2);
        });
    }
}