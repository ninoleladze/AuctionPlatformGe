using AuctionPlatformGe.Application.Features.Bids.Commands.PlaceBid;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace AuctionPlatformGe.API.Hubs;

[Authorize]
public class BidHub : Hub
{
    private const int MaxBidsPerMinute = 20;
    private readonly IMediator _mediator;
    private readonly IMemoryCache _cache;

    public BidHub(IMediator mediator, IMemoryCache cache)
    {
        _mediator = mediator;
        _cache    = cache;
    }

    public async Task JoinAuction(string auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
    }

    public async Task LeaveAuction(string auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
    }

    public async Task PlaceBid(string auctionId, decimal amount)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            await Clients.Caller.SendAsync("BidError", "Not authenticated");
            return;
        }

        // Fixed-window rate limit: max 20 bids per minute per user
        var rateLimitKey = $"bid-rl:{userId}:{DateTime.UtcNow:yyyyMMddHHmm}";
        var bidCount = _cache.GetOrCreate<int>(rateLimitKey, entry =>
        {
            entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(2);
            return 0;
        });

        if (bidCount >= MaxBidsPerMinute)
        {
            await Clients.Caller.SendAsync("BidError", "Rate limit exceeded. Max 20 bids per minute.");
            return;
        }
        _cache.Set(rateLimitKey, bidCount + 1);

        var ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();

        try
        {
            var result = await _mediator.Send(new PlaceBidCommand(auctionId, amount, userId, ipAddress));
            await Clients.Group($"auction-{auctionId}").SendAsync("BidPlaced", result);
        }
        catch (ValidationException ex)
        {
            var message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
            await Clients.Caller.SendAsync("BidError", message);
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.SendAsync("BidError", ex.Message);
        }
    }
}
