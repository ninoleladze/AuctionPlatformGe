using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Application.Models;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Payments.Commands.InitiatePayment;

public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, PaymentInitDto>
{
    private readonly IPaymentGateway _gateway;

    public InitiatePaymentCommandHandler(IPaymentGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<PaymentInitDto> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        var package = BidPackages.GetById(request.PackageId)
            ?? throw new InvalidOperationException($"Bid package {request.PackageId} not found.");

        var transactionId = Guid.NewGuid().ToString("N");

        var result = await _gateway.InitiatePaymentAsync(new PaymentRequest(
            transactionId,
            request.UserId,
            package.PriceGel,
            $"{package.Name} — {package.BidCount} bids",
            request.ReturnUrl,
            request.CallbackUrl));

        if (!result.Success)
            throw new InvalidOperationException(result.Error ?? "Payment gateway error.");

        return new PaymentInitDto(transactionId, result.PaymentUrl!);
    }
}
