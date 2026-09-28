using AuctionPlatformGe.Application.DTOs;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Payments.Commands.InitiatePayment;

public record InitiatePaymentCommand(string UserId, int PackageId, string ReturnUrl, string CallbackUrl)
    : IRequest<PaymentInitDto>;
