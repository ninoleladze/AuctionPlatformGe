using AutoMapper;
using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Auctions.Commands.CreateAuction;

public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, AuctionDetailDto>
{
    private readonly IAuctionRepository _auctions;
    private readonly IMapper _mapper;

    public CreateAuctionCommandHandler(IAuctionRepository auctions, IMapper mapper)
    {
        _auctions = auctions;
        _mapper = mapper;
    }

    public async Task<AuctionDetailDto> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = new Auction
        {
            Title = request.Title,
            Description = request.Description,
            ItemType = request.ItemType,
            StartingPrice = request.StartingPrice,
            CurrentBid = request.StartingPrice,
            ReservePrice = request.ReservePrice,
            BuyNowPrice = request.BuyNowPrice,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Status = "Active",
            SellerId = request.SellerId,
            ImageUrl = request.ImageUrl
        };

        await _auctions.AddAsync(auction);
        await _auctions.SaveChangesAsync();

        return _mapper.Map<AuctionDetailDto>(auction);
    }
}
