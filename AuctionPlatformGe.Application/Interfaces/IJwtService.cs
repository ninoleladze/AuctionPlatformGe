using AuctionPlatformGe.Core.Entities;

namespace AuctionPlatformGe.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(AppUser user);
}
