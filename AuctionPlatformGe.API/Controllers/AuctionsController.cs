using AuctionPlatformGe.Application.Features.Auctions.Commands.CreateAuction;
using AuctionPlatformGe.Application.Features.Auctions.Queries.GetAllAuctions;
using AuctionPlatformGe.Application.Features.Auctions.Queries.GetAuctionById;
using AuctionPlatformGe.Application.Features.Auctions.Queries.GetMyAuctions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuctionPlatformGe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuctionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 12) =>
        Ok(await _mediator.Send(new GetAllAuctionsQuery(page, pageSize)));

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _mediator.Send(new GetMyAuctionsQuery(sellerId)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAuctionByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateAuctionDto dto)
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new CreateAuctionCommand(
            dto.Title,
            dto.Description,
            dto.ItemType,
            dto.StartingPrice,
            dto.ReservePrice,
            dto.BuyNowPrice,
            dto.StartsAt,
            dto.EndsAt,
            sellerId,
            dto.ImageUrl
        ));
        return Ok(result);
    }
}

public record CreateAuctionDto(
    string Title,
    string Description,
    string ItemType,
    decimal StartingPrice,
    decimal? ReservePrice,
    decimal? BuyNowPrice,
    DateTime StartsAt,
    DateTime EndsAt,
    string? ImageUrl = null
);
