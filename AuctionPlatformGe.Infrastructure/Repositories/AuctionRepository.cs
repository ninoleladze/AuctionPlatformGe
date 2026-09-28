using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using AuctionPlatformGe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatformGe.Infrastructure.Repositories;

public class AuctionRepository : Repository<Auction>, IAuctionRepository
{
    public AuctionRepository(AppDbContext context) : base(context) { }

    public async Task<Auction?> GetWithBidsAsync(Guid id) =>
        await _context.Auctions
            .Include(a => a.Bids)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<(IReadOnlyList<Auction> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _context.Auctions.AsQueryable();
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }
}
