using AutoMapper;
using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Interfaces;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Auctions.Queries.GetAllAuctions;

public class GetAllAuctionsQueryHandler : IRequestHandler<GetAllAuctionsQuery, PaginatedResult<AuctionListDto>>
{
    private readonly IAuctionRepository _auctions;
    private readonly IMapper _mapper;

    public GetAllAuctionsQueryHandler(IAuctionRepository auctions, IMapper mapper)
    {
        _auctions = auctions;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<AuctionListDto>> Handle(GetAllAuctionsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _auctions.GetPagedAsync(request.Page, request.PageSize);
        return new PaginatedResult<AuctionListDto>(
            _mapper.Map<IReadOnlyList<AuctionListDto>>(items),
            total,
            request.Page,
            request.PageSize
        );
    }
}
