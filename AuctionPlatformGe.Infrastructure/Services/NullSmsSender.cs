using AuctionPlatformGe.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuctionPlatformGe.Infrastructure.Services;

/// <summary>Development-only SMS sender that logs codes to the console instead of sending real SMS.</summary>
public class NullSmsSender : ISmsSender
{
    private readonly ILogger<NullSmsSender> _logger;

    public NullSmsSender(ILogger<NullSmsSender> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendOtpAsync(string phoneNumber, string code)
    {
        _logger.LogWarning("[SMS-NULL] OTP for {Phone}: {Code} (dev mode — not sent)", phoneNumber, code);
        return Task.FromResult(true);
    }
}
