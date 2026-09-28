namespace AuctionPlatformGe.Application.Interfaces;

public record PaymentRequest(
    string TransactionId,
    string UserId,
    decimal AmountGel,
    string Description,
    string ReturnUrl,
    string CallbackUrl,
    string Purpose = "BidPurchase",
    Guid? AuctionId = null);

public record PaymentInitResult(bool Success, string? PaymentUrl, string? Error);

public record WebhookPaymentResult(
    string TransactionId,
    bool IsSuccess,
    decimal Amount,
    string? RawStatus);

public interface IPaymentGateway
{
    Task<PaymentInitResult> InitiatePaymentAsync(PaymentRequest request);
    bool VerifyWebhookSignature(string rawPayload, string signature);
    WebhookPaymentResult ParseWebhook(string rawPayload);
}
