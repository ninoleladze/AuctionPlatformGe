using AuctionPlatformGe.Application.Features.AutoBids.Commands.CancelAutoBid;
using AuctionPlatformGe.Application.Features.AutoBids.Commands.SetAutoBid;
using AuctionPlatformGe.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuctionPlatformGe.API.Controllers;

[ApiController]
[Route("api/auto-bids")]
[Authorize]
public class AutoBidsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAutoBidRepository _autoBids;

    public AutoBidsController(IMediator mediator, IAutoBidRepository autoBids)
    {
        _mediator = mediator;
        _autoBids = autoBids;
    }

    /// <summary>Get the caller's active auto-bid for an auction (null if none).</summary>
    [HttpGet("{auctionId}")]
    public async Task<IActionResult> Get(Guid auctionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var autoBid = await _autoBids.GetActiveAsync(auctionId, userId);
        return Ok(autoBid is null ? null : new { autoBid.MaxAmount });
    }

    /// <summary>Set or update an auto-bid for an auction.</summary>
    [HttpPost]
    public async Task<IActionResult> Set(SetAutoBidDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _mediator.Send(new SetAutoBidCommand(dto.AuctionId, userId, dto.MaxAmount));
        return Ok();
    }

    /// <summary>Cancel the caller's active auto-bid for an auction.</summary>
    [HttpDelete("{auctionId}")]
    public async Task<IActionResult> Cancel(string auctionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _mediator.Send(new CancelAutoBidCommand(auctionId, userId));
        return Ok();
    }
}

public record SetAutoBidDto(string AuctionId, decimal MaxAmount);
