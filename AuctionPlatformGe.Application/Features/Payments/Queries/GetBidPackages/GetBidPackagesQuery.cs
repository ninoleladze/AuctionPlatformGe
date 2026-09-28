using AuctionPlatformGe.Application.DTOs;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Payments.Queries.GetBidPackages;

public record GetBidPackagesQuery : IRequest<List<BidPackageDto>>;
