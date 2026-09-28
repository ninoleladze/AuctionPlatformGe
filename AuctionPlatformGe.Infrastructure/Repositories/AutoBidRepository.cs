using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using AuctionPlatformGe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatformGe.Infrastructure.Repositories;

public class AutoBidRepository : Repository<AutoBid>, IAutoBidRepository
{
    public AutoBidRepository(AppDbContext context) : base(context) { }

    public async Task<AutoBid?> GetActiveAsync(Guid auctionId, string userId) =>
        await _context.AutoBids
            .FirstOrDefaultAsync(ab => ab.AuctionId == auctionId && ab.UserId == userId && ab.IsActive);

    public async Task<IList<AutoBid>> GetActiveForAuctionAsync(Guid auctionId) =>
        await _context.AutoBids
            .Where(ab => ab.AuctionId == auctionId && ab.IsActive)
            .ToListAsync();
}
