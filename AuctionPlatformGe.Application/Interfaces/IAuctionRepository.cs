using AuctionPlatformGe.Core.Entities;

namespace AuctionPlatformGe.Application.Interfaces;

public interface IAuctionRepository : IRepository<Auction>
{
    Task<Auction?> GetWithBidsAsync(Guid id);
    Task<(IReadOnlyList<Auction> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
}
