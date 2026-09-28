using System.Data;
using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Features.Bids.Commands.PlaceBid;
using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using Moq;
using Xunit;

namespace AuctionPlatformGe.Tests.Bids;

public class PlaceBidCommandHandlerTests
{
    private readonly Mock<IAuctionRepository> _auctions  = new();
    private readonly Mock<IBidRepository>    _bids      = new();
    private readonly Mock<IWalletRepository> _wallets   = new();
    private readonly Mock<IAutoBidRepository> _autoBids = new();
    private readonly Mock<IUnitOfWork>       _uow       = new();
    private readonly PlaceBidCommandHandler  _handler;

    public PlaceBidCommandHandlerTests()
    {
        // Execute the delegate directly — no real DB transaction in unit tests
        _uow.Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<Task<BidResultDto>>>(),
                It.IsAny<IsolationLevel>()))
            .Returns<Func<Task<BidResultDto>>, IsolationLevel>((op, _) => op());

        _handler = new PlaceBidCommandHandler(
            _auctions.Object, _bids.Object, _wallets.Object,
            _autoBids.Object, _uow.Object);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Auction MakeAuction(
        string auctionId, string sellerId,
        decimal currentBid = 10m, int bidCount = 0,
        bool ended = false) => new()
    {
        Id             = Guid.Parse(auctionId),
        SellerId       = sellerId,
        CurrentBid     = currentBid,
        BidCount       = bidCount,
        Status         = "Active",
        StartsAt       = DateTime.UtcNow.AddDays(-1),
        EndsAt         = ended ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddHours(1),
        Title          = "Test Auction",
        Bids           = [],
        AutoBids       = [],
    };

    private static Wallet MakeWallet(string userId, int bidBalance) => new()
    {
        Id           = Guid.NewGuid(),
        UserId       = userId,
        BidBalance   = bidBalance,
        Transactions = [],
    };

    private void SetupSuccessfulBidDeps(string auctionId, Auction auction, Wallet wallet)
    {
        _auctions.Setup(r => r.GetWithBidsAsync(Guid.Parse(auctionId))).ReturnsAsync(auction);
        _wallets.Setup(r => r.GetByUserIdAsync(It.IsAny<string>())).ReturnsAsync(wallet);
        _autoBids.Setup(r => r.GetActiveForAuctionAsync(Guid.Parse(auctionId))).ReturnsAsync([]);
        _bids.Setup(r => r.AddAsync(It.IsAny<Bid>())).Returns(Task.CompletedTask);
        _bids.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
    }

    // ── Tests ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidBid_ReturnsBidResultWithIncrementedCount()
    {
        var auctionId = Guid.NewGuid().ToString();
        var auction   = MakeAuction(auctionId, "seller-1", currentBid: 10m, bidCount: 3);
        var wallet    = MakeWallet("user-1", 5);

        SetupSuccessfulBidDeps(auctionId, auction, wallet);

        var result = await _handler.Handle(
            new PlaceBidCommand(auctionId, 15m, "user-1", null), default);

        Assert.Equal(15m,    result.NewBid);
        Assert.Equal(4,      result.BidCount);   // 3 + 1
        Assert.Equal("user-1", result.BidderId);
        Assert.Equal(4,      auction.BidCount);  // entity updated in-place
        Assert.Equal(4,      wallet.BidBalance); // 5 - 1
    }

    [Fact]
    public async Task Handle_BidTooLow_ThrowsInvalidOperationException()
    {
        var auctionId = Guid.NewGuid().ToString();
        var auction   = MakeAuction(auctionId, "seller-1", currentBid: 50m);
        var wallet    = MakeWallet("user-1", 5);

        SetupSuccessfulBidDeps(auctionId, auction, wallet);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new PlaceBidCommand(auctionId, 30m, "user-1", null), default));
    }

    [Fact]
    public async Task Handle_EqualBid_ThrowsInvalidOperationException()
    {
        var auctionId = Guid.NewGuid().ToString();
        var auction   = MakeAuction(auctionId, "seller-1", currentBid: 50m);
        var wallet    = MakeWallet("user-1", 5);

        SetupSuccessfulBidDeps(auctionId, auction, wallet);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new PlaceBidCommand(auctionId, 50m, "user-1", null), default));
    }

    [Fact]
    public async Task Handle_SellerBid_ThrowsInvalidOperationException()
    {
        var auctionId = Guid.NewGuid().ToString();
        var sellerId  = "seller-1";
        var auction   = MakeAuction(auctionId, sellerId, currentBid: 10m);
        var wallet    = MakeWallet(sellerId, 5);

        SetupSuccessfulBidDeps(auctionId, auction, wallet);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new PlaceBidCommand(auctionId, 20m, sellerId, null), default));
    }

    [Fact]
    public async Task Handle_ZeroBidBalance_ThrowsInvalidOperationException()
    {
        var auctionId = Guid.NewGuid().ToString();
        var auction   = MakeAuction(auctionId, "seller-1", currentBid: 10m);
        var wallet    = MakeWallet("user-1", 0);

        SetupSuccessfulBidDeps(auctionId, auction, wallet);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new PlaceBidCommand(auctionId, 20m, "user-1", null), default));
    }

    [Fact]
    public async Task Handle_AuctionAlreadyEnded_ThrowsInvalidOperationException()
    {
        var auctionId = Guid.NewGuid().ToString();
        var auction   = MakeAuction(auctionId, "seller-1", currentBid: 10m, ended: true);
        var wallet    = MakeWallet("user-1", 5);

        SetupSuccessfulBidDeps(auctionId, auction, wallet);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new PlaceBidCommand(auctionId, 20m, "user-1", null), default));
    }

    [Fact]
    public async Task Handle_AutoBidCounterFires_BidCountIncrementedTwice()
    {
        var auctionId  = Guid.NewGuid().ToString();
        var auction    = MakeAuction(auctionId, "seller-1", currentBid: 10m, bidCount: 0);
        var bidderWallet   = MakeWallet("user-1", 5);
        var autoBidderWallet = MakeWallet("auto-user", 5);

        var autoBid = new AutoBid
        {
            Id        = Guid.NewGuid(),
            AuctionId = Guid.Parse(auctionId),
            UserId    = "auto-user",
            MaxAmount = 100m,
            IsActive  = true
        };

        _auctions.Setup(r => r.GetWithBidsAsync(Guid.Parse(auctionId))).ReturnsAsync(auction);
        _wallets.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync(bidderWallet);
        _wallets.Setup(r => r.GetByUserIdAsync("auto-user")).ReturnsAsync(autoBidderWallet);
        _autoBids.Setup(r => r.GetActiveForAuctionAsync(Guid.Parse(auctionId)))
                 .ReturnsAsync([autoBid]);
        _bids.Setup(r => r.AddAsync(It.IsAny<Bid>())).Returns(Task.CompletedTask);
        _bids.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new PlaceBidCommand(auctionId, 15m, "user-1", null), default);

        // Manual bid (user-1) + auto-bid counter (auto-user) = 2 bids
        Assert.Equal(2, result.BidCount);
        Assert.Equal("auto-user", result.BidderId);
        Assert.Equal(16m, result.NewBid); // manual 15 + 1
    }
}
