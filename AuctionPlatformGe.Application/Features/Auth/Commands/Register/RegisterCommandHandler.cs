using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _users;
    private readonly IWalletRepository _wallets;
    private readonly IJwtService _jwt;

    public RegisterCommandHandler(IUserRepository users, IWalletRepository wallets, IJwtService jwt)
    {
        _users = users;
        _wallets = wallets;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _users.ExistsAsync(request.Email))
            throw new InvalidOperationException("Email already exists");

        var user = new AppUser
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _users.AddAsync(user);

        var wallet = new Wallet
        {
            UserId = user.Id,
            BidBalance = 5  // 5 free bids on registration
        };
        await _wallets.AddAsync(wallet);

        await _users.SaveChangesAsync();

        return new AuthResponseDto(_jwt.GenerateToken(user), user.Username, user.Email, user.Id, 5, user.Role);
    }
}
