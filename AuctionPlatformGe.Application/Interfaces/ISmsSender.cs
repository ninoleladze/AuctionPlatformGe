namespace AuctionPlatformGe.Application.Interfaces;

public interface ISmsSender
{
    Task<bool> SendOtpAsync(string phoneNumber, string code);
}
