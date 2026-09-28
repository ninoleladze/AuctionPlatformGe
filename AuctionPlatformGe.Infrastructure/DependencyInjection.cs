using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Infrastructure.Data;
using AuctionPlatformGe.Infrastructure.Repositories;
using AuctionPlatformGe.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionPlatformGe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database");

        services.AddScoped<IAuctionRepository, AuctionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBidRepository, BidRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IAutoBidRepository, AutoBidRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddMemoryCache();
        services.AddScoped<IPaymentGateway, PayzePaymentGateway>();
        services.AddScoped<ISmsSender, NullSmsSender>();

        return services;
    }
}
