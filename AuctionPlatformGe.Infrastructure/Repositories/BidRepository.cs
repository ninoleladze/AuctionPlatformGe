using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using AuctionPlatformGe.Infrastructure.Data;

namespace AuctionPlatformGe.Infrastructure.Repositories;

public class BidRepository : Repository<Bid>, IBidRepository
{
    public BidRepository(AppDbContext context) : base(context) { }
}
