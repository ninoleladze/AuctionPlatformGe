using AutoMapper;
using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Core.Entities;

namespace AuctionPlatformGe.Application.Mappings;

public class AuctionProfile : Profile
{
    public AuctionProfile()
    {
        CreateMap<Bid, BidDto>();

        CreateMap<Auction, AuctionListDto>()
            .ConstructUsing((src, ctx) => new AuctionListDto(
                src.Id,
                src.Title,
                src.Description,
                src.ItemType,
                src.StartingPrice,
                src.CurrentBid,
                src.BuyNowPrice,
                src.Status,
                src.StartsAt,
                src.EndsAt,
                src.SellerId,
                src.BidCount,
                src.ImageUrl
            ));

        CreateMap<Auction, AuctionDetailDto>()
            .ConstructUsing((src, ctx) => new AuctionDetailDto(
                src.Id,
                src.Title,
                src.Description,
                src.ItemType,
                src.StartingPrice,
                src.CurrentBid,
                src.BuyNowPrice,
                src.Status,
                src.StartsAt,
                src.EndsAt,
                src.SellerId,
                src.HighestBidderId,
                src.BidCount,
                ctx.Mapper.Map<IList<BidDto>>(src.Bids),
                src.ImageUrl
            ));
    }
}
