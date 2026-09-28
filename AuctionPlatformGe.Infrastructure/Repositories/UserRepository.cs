using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using AuctionPlatformGe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatformGe.Infrastructure.Repositories;

public class UserRepository : Repository<AppUser>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<AppUser?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> ExistsAsync(string email) =>
        await _context.Users.AnyAsync(u => u.Email == email);
}
