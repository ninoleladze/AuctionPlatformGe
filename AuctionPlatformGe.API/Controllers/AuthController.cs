using AuctionPlatformGe.Application.Features.Auth.Commands.Login;
using AuctionPlatformGe.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuctionPlatformGe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto) =>
        Ok(await _mediator.Send(new RegisterCommand(dto.Username, dto.Email, dto.Password)));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto) =>
        Ok(await _mediator.Send(new LoginCommand(dto.Email, dto.Password)));
}

public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Email, string Password);
