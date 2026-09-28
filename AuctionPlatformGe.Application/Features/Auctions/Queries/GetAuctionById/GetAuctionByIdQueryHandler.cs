using AutoMapper;
using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Interfaces;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Auctions.Queries.GetAuctionById;

public class GetAuctionByIdQueryHandler : IRequestHandler<GetAuctionByIdQuery, AuctionDetailDto?>
{
    private readonly IAuctionRepository _auctions;
    private readonly IMapper _mapper;

    public GetAuctionByIdQueryHandler(IAuctionRepository auctions, IMapper mapper)
    {
        _auctions = auctions;
        _mapper = mapper;
    }

    public async Task<AuctionDetailDto?> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
    {
        var auction = await _auctions.GetWithBidsAsync(request.Id);
        return auction is null ? null : _mapper.Map<AuctionDetailDto>(auction);
    }
}
