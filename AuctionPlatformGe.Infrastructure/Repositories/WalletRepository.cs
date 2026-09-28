using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using AuctionPlatformGe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatformGe.Infrastructure.Repositories;

public class WalletRepository : Repository<Wallet>, IWalletRepository
{
    public WalletRepository(AppDbContext context) : base(context) { }

    public async Task<Wallet?> GetByUserIdAsync(string userId) =>
        await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId);
}
