using FluentValidation;

namespace AuctionPlatformGe.Application.Features.AutoBids.Commands.SetAutoBid;

public class SetAutoBidCommandValidator : AbstractValidator<SetAutoBidCommand>
{
    public SetAutoBidCommandValidator()
    {
        RuleFor(x => x.AuctionId).NotEmpty().Must(id => Guid.TryParse(id, out _)).WithMessage("Invalid auction ID");
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.MaxAmount).GreaterThan(0).WithMessage("Max amount must be positive");
    }
}
