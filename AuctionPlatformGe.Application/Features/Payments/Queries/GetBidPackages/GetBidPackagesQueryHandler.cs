using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Models;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Payments.Queries.GetBidPackages;

public class GetBidPackagesQueryHandler : IRequestHandler<GetBidPackagesQuery, List<BidPackageDto>>
{
    public Task<List<BidPackageDto>> Handle(GetBidPackagesQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(BidPackages.All
            .Select(p => new BidPackageDto(p.Id, p.BidCount, p.PriceGel, p.Name))
            .ToList());
}
