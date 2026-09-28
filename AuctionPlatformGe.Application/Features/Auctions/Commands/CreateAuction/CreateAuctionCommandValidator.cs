using FluentValidation;

namespace AuctionPlatformGe.Application.Features.Auctions.Commands.CreateAuction;

public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    private static readonly string[] ValidItemTypes = ["Physical", "Digital", "GameItem"];

    public CreateAuctionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.ItemType)
            .NotEmpty().WithMessage("Item type is required.")
            .Must(t => ValidItemTypes.Contains(t))
            .WithMessage("ItemType must be Physical, Digital, or GameItem.");

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0).WithMessage("Starting price must be greater than 0.");

        RuleFor(x => x.ReservePrice)
            .GreaterThanOrEqualTo(x => x.StartingPrice)
            .WithMessage("Reserve price must be greater than or equal to the starting price.")
            .When(x => x.ReservePrice.HasValue);

        RuleFor(x => x.BuyNowPrice)
            .GreaterThan(x => x.StartingPrice)
            .WithMessage("Buy-now price must be greater than the starting price.")
            .When(x => x.BuyNowPrice.HasValue);

        RuleFor(x => x.StartsAt)
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddSeconds(-30))
            .WithMessage("Start time cannot be in the past.");

        RuleFor(x => x.EndsAt)
            .GreaterThan(x => x.StartsAt)
            .WithMessage("End time must be after start time.");

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");
    }
}
