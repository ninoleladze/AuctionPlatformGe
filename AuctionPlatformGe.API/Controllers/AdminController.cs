using AuctionPlatformGe.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatformGe.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db) => _db = db;

    /// <summary>Platform-wide stats for the dashboard.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var totalUsers    = await _db.Users.CountAsync();
        var totalAuctions = await _db.Auctions.CountAsync();
        var active        = await _db.Auctions.CountAsync(a => a.Status == "Active");
        var ended         = await _db.Auctions.CountAsync(a => a.Status == "Ended");
        var draft         = await _db.Auctions.CountAsync(a => a.Status == "Draft");
        var totalBids     = await _db.Bids.CountAsync();
        var totalRevenue  = await _db.PendingPayments
            .Where(p => p.Status == "Completed" && p.Purpose == "BidPurchase")
            .SumAsync(p => (decimal?)p.AmountGel) ?? 0m;
        var buyNowRevenue = await _db.PendingPayments
            .Where(p => p.Status == "Completed" && p.Purpose == "BuyNow")
            .SumAsync(p => (decimal?)p.AmountGel) ?? 0m;

        return Ok(new
        {
            totalUsers,
            totalAuctions,
            active,
            ended,
            draft,
            totalBids,
            totalRevenue      = Math.Round(totalRevenue, 2),
            buyNowRevenue     = Math.Round(buyNowRevenue, 2),
            combinedRevenue   = Math.Round(totalRevenue + buyNowRevenue, 2)
        });
    }

    /// <summary>All users with role info.</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.CreatedAt,
                u.PhoneVerified,
                BidBalance = u.Wallet != null ? u.Wallet.BidBalance : 0
            })
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>Set a user's role (User | Admin).</summary>
    [HttpPatch("users/{id}/role")]
    public async Task<IActionResult> SetRole(string id, [FromBody] SetRoleDto dto)
    {
        if (dto.Role != "User" && dto.Role != "Admin")
            return BadRequest(new { error = "Role must be 'User' or 'Admin'." });

        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.Role = dto.Role;
        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.Role });
    }

    /// <summary>All auctions for admin management.</summary>
    [HttpGet("auctions")]
    public async Task<IActionResult> GetAuctions()
    {
        var auctions = await _db.Auctions
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.ItemType,
                a.Status,
                a.CurrentBid,
                a.BuyNowPrice,
                a.BidCount,
                a.StartsAt,
                a.EndsAt,
                a.SellerId,
                a.CreatedAt
            })
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(auctions);
    }

    /// <summary>Force-activate a Draft auction immediately.</summary>
    [HttpPost("auctions/{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var auction = await _db.Auctions.FindAsync(id);
        if (auction is null) return NotFound();
        if (auction.Status != "Draft")
            return BadRequest(new { error = "Only Draft auctions can be activated." });

        auction.Status   = "Active";
        auction.StartsAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { auction.Id, auction.Status });
    }

    /// <summary>Cancel an auction (Draft or Active).</summary>
    [HttpDelete("auctions/{id}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var auction = await _db.Auctions.FindAsync(id);
        if (auction is null) return NotFound();
        if (auction.Status == "Ended")
            return BadRequest(new { error = "Cannot cancel an already-ended auction." });

        auction.Status = "Cancelled";
        await _db.SaveChangesAsync();
        return Ok(new { auction.Id, auction.Status });
    }
}

public record SetRoleDto(string Role);
