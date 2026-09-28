using FluentValidation;

namespace AuctionPlatformGe.Application.Features.Bids.Commands.PlaceBid;

public class PlaceBidCommandValidator : AbstractValidator<PlaceBidCommand>
{
    public PlaceBidCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage("Auction ID is required.")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Auction ID must be a valid GUID.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid amount must be greater than 0.");

        RuleFor(x => x.BidderId)
            .NotEmpty().WithMessage("Bidder ID is required.");
    }
}
