using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Interfaces;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _users;
    private readonly IWalletRepository _wallets;
    private readonly IJwtService _jwt;

    public LoginCommandHandler(IUserRepository users, IWalletRepository wallets, IJwtService jwt)
    {
        _users = users;
        _wallets = wallets;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var wallet = await _wallets.GetByUserIdAsync(user.Id);

        return new AuthResponseDto(_jwt.GenerateToken(user), user.Username, user.Email, user.Id, wallet?.BidBalance ?? 0, user.Role);
    }
}
