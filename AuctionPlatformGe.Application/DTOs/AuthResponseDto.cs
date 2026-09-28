namespace AuctionPlatformGe.Application.DTOs;

public record AuthResponseDto(string Token, string Username, string Email, string UserId, int BidBalance, string Role = "User");
