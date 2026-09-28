using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using AuctionPlatformGe.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuctionPlatformGe.Infrastructure.Services;

public class PayzePaymentGateway : IPaymentGateway
{
    private readonly IConfiguration _config;
    private readonly ILogger<PayzePaymentGateway> _logger;
    private readonly AppDbContext _db;
    private readonly bool _testMode;
    private readonly string _webhookSecret;

    public PayzePaymentGateway(IConfiguration config, ILogger<PayzePaymentGateway> logger, AppDbContext db)
    {
        _config = config;
        _logger = logger;
        _db = db;
        _testMode = config.GetValue<bool>("Payze:TestMode", true);
        _webhookSecret = config["Payze:WebhookSecret"] ?? "test-webhook-secret";
    }

    public async Task<PaymentInitResult> InitiatePaymentAsync(PaymentRequest request)
    {
        // Persist pending payment
        _db.PendingPayments.Add(new PendingPayment
        {
            UserId        = request.UserId,
            TransactionId = request.TransactionId,
            AmountGel     = request.AmountGel,
            Purpose       = request.Purpose,
            AuctionId     = request.AuctionId,
            Status        = "Pending"
        });
        await _db.SaveChangesAsync();

        if (_testMode)
        {
            // In test mode return a URL to our own test-complete endpoint
            var testUrl = $"{request.CallbackUrl.Replace("/webhook/payze", "/test-complete")}?transactionId={request.TransactionId}";
            _logger.LogInformation("[Payze TEST] Payment initiated: {TxId} — redirect to {Url}", request.TransactionId, testUrl);
            return new PaymentInitResult(true, testUrl, null);
        }

        // Production: call Payze API
        try
        {
            using var http = new HttpClient();
            var apiKey    = _config["Payze:ApiKey"]!;
            var apiSecret = _config["Payze:ApiSecret"]!;

            var payload = new
            {
                source  = new { amount = request.AmountGel, currency = "GEL" },
                hooks   = new { webhookGateway = request.CallbackUrl, successRedirectGateway = request.ReturnUrl },
                comments = request.Description,
                orderId  = request.TransactionId
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            http.DefaultRequestHeaders.Add("Authorization", $"{apiKey}:{apiSecret}");
            var response = await http.PostAsync("https://payze.io/api/v1/payment/justpay", content);
            var body     = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[Payze] Error: {Status} {Body}", response.StatusCode, body);
                return new PaymentInitResult(false, null, $"Payze error: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(body);
            var paymentUrl = doc.RootElement.GetProperty("data").GetProperty("payment_url").GetString();
            return new PaymentInitResult(true, paymentUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Payze] Exception initiating payment");
            return new PaymentInitResult(false, null, "Payment gateway unavailable.");
        }
    }

    public bool VerifyWebhookSignature(string rawPayload, string signature)
    {
        if (_testMode) return true;

        var key  = Encoding.UTF8.GetBytes(_webhookSecret);
        var data = Encoding.UTF8.GetBytes(rawPayload);
        var hash = Convert.ToHexString(HMACSHA256.HashData(key, data)).ToLowerInvariant();
        return hash == signature?.ToLowerInvariant();
    }

    public WebhookPaymentResult ParseWebhook(string rawPayload)
    {
        using var doc    = JsonDocument.Parse(rawPayload);
        var root         = doc.RootElement;
        var transactionId = root.GetProperty("orderId").GetString() ?? string.Empty;
        var status        = root.GetProperty("status").GetString() ?? string.Empty;
        var amount        = root.GetProperty("amount").GetDecimal();

        return new WebhookPaymentResult(transactionId, status == "completed", amount, status);
    }
}
