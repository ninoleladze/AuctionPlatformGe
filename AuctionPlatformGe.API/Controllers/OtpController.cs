using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuctionPlatformGe.API.Controllers;

[ApiController]
[Route("api/auth/phone")]
[Authorize]
[EnableRateLimiting("auth")]
public class OtpController : ControllerBase
{
    private const int MaxAttempts = 5;
    private readonly ISmsSender _sms;
    private readonly IMemoryCache _cache;
    private readonly AppDbContext _db;

    public OtpController(ISmsSender sms, IMemoryCache cache, AppDbContext db)
    {
        _sms   = sms;
        _cache = cache;
        _db    = db;
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
    {
        var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cacheKey = $"otp:{userId}";

        // Cryptographically secure 6-digit code
        var code = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

        _cache.Set(cacheKey, (Code: code, Phone: dto.PhoneNumber, Attempts: 0),
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        await _sms.SendOtpAsync(dto.PhoneNumber, code);
        return Ok(new { message = "OTP sent." });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cacheKey = $"otp:{userId}";

        if (!_cache.TryGetValue(cacheKey, out (string Code, string Phone, int Attempts) cached))
            return BadRequest(new { error = "OTP expired or not requested." });

        // Increment attempt count first to prevent timing side-channels
        var updatedAttempts = cached.Attempts + 1;
        if (updatedAttempts > MaxAttempts)
        {
            _cache.Remove(cacheKey);
            return BadRequest(new { error = "Too many attempts. Please request a new OTP." });
        }

        // Update attempt counter back into cache
        _cache.Set(cacheKey, (cached.Code, cached.Phone, updatedAttempts),
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        if (cached.Code != dto.Code)
            return BadRequest(new { error = $"Invalid OTP. {MaxAttempts - updatedAttempts} attempt(s) remaining." });

        // Correct — update user and clear cache
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is not null)
        {
            user.PhoneNumber   = cached.Phone;
            user.PhoneVerified = true;
            await _db.SaveChangesAsync();
        }

        _cache.Remove(cacheKey);
        return Ok(new { message = "Phone verified." });
    }
}

public record SendOtpDto(string PhoneNumber);
public record VerifyOtpDto(string Code);
