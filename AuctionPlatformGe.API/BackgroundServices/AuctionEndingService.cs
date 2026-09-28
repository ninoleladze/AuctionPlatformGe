using AuctionPlatformGe.API.Hubs;
using AuctionPlatformGe.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatformGe.API.BackgroundServices;

public class AuctionEndingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<BidHub> _hub;
    private readonly ILogger<AuctionEndingService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    public AuctionEndingService(
        IServiceScopeFactory scopeFactory,
        IHubContext<BidHub> hub,
        ILogger<AuctionEndingService> logger)
    {
        _scopeFactory = scopeFactory;
        _hub          = hub;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await TickAsync(stoppingToken); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuctionEndingService error");
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db  = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;

        // ── 1. Draft → Active when StartsAt has arrived ───────────────────────
        var toActivate = await db.Auctions
            .Where(a => a.Status == "Draft" && a.StartsAt <= now)
            .ToListAsync(cancellationToken);

        if (toActivate.Count > 0)
        {
            foreach (var a in toActivate)
                a.Status = "Active";

            await db.SaveChangesAsync(cancellationToken);

            foreach (var a in toActivate)
        {
            _logger.LogInformation("Auction {Id} ({Title}) is now Active", a.Id, a.Title);
            await _hub.Clients.All.SendAsync("AuctionActivated", new
            {
                auctionId = a.Id.ToString(),
                title     = a.Title
            }, cancellationToken);
        }
        }

        // ── 2. Active → Ended when EndsAt has passed ──────────────────────────
        var toEnd = await db.Auctions
            .Where(a => a.Status == "Active" && a.EndsAt <= now)
            .ToListAsync(cancellationToken);

        if (toEnd.Count == 0) return;

        var reserveNotMetIds = new HashSet<Guid>();

        foreach (var auction in toEnd)
        {
            bool reserveNotMet = auction.ReservePrice.HasValue
                && auction.CurrentBid < auction.ReservePrice.Value;

            auction.Status = "Ended";

            if (reserveNotMet)
            {
                auction.HighestBidderId = null;   // no winner — reserve not met
                reserveNotMetIds.Add(auction.Id);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var auction in toEnd)
        {
            bool reserveNotMet = reserveNotMetIds.Contains(auction.Id);

            _logger.LogInformation(
                "Auction {Id} ({Title}) ended. FinalBid={Bid} Winner={Winner} ReserveNotMet={ReserveNotMet}",
                auction.Id, auction.Title, auction.CurrentBid,
                auction.HighestBidderId ?? "none", reserveNotMet);

            await _hub.Clients
                .Group($"auction-{auction.Id}")
                .SendAsync("AuctionEnded", new
                {
                    auctionId       = auction.Id.ToString(),
                    finalBid        = auction.CurrentBid,
                    highestBidderId = auction.HighestBidderId,
                    reserveNotMet
                }, cancellationToken);
        }
    }
}
