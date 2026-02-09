using FraudDetection.Core.Interfaces;
using FraudDetection.Core.Services;
using FraudDetection.Infrastructure.Data;
using FraudDetection.Infrastructure.Repositories;
using FraudDetection.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FraudDetection.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserBehaviorProfileRepository, UserBehaviorProfileRepository>();
        
        services.AddScoped<IFraudScoringService, FraudScoringService>();

        return services;
    }
}