using AuctionPlatformGe.Core.Entities;

namespace AuctionPlatformGe.Application.Interfaces;

public interface IUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByEmailAsync(string email);
    Task<bool> ExistsAsync(string email);
}
