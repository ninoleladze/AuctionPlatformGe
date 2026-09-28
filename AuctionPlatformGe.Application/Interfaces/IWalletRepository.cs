using AuctionPlatformGe.Core.Entities;

namespace AuctionPlatformGe.Application.Interfaces;

public interface IWalletRepository : IRepository<Wallet>
{
    Task<Wallet?> GetByUserIdAsync(string userId);
}
