using AuctionPlatformGe.Application.Features.Bids.Queries.GetUserBids;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuctionPlatformGe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BidsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BidsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await _mediator.Send(
            new GetUserBidsQuery(User.FindFirstValue(ClaimTypes.NameIdentifier)!, page, pageSize)));
}
