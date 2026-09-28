using AuctionPlatformGe.Application.DTOs;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Payments.Commands.InitiateBuyNow;

public record InitiateBuyNowCommand(
    string UserId,
    string AuctionId,
    string ReturnUrl,
    string CallbackUrl) : IRequest<PaymentInitDto>;
