using AuctionPlatformGe.Core.Entities;

namespace AuctionPlatformGe.Application.Interfaces;

public interface IAutoBidRepository : IRepository<AutoBid>
{
    Task<AutoBid?> GetActiveAsync(Guid auctionId, string userId);
    Task<IList<AutoBid>> GetActiveForAuctionAsync(Guid auctionId);
}
