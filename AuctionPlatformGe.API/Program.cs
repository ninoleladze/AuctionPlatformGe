using AuctionPlatformGe.Application;
using AuctionPlatformGe.Application.Behaviors;
using AuctionPlatformGe.Application.Mappings;
using AuctionPlatformGe.Infrastructure;
using AuctionPlatformGe.API.BackgroundServices;
using AuctionPlatformGe.API.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using AuctionPlatformGe.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ───────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: ctx.HostingEnvironment.IsDevelopment()
            ? "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}\n  {Message:lj}{NewLine}{Exception}"
            : "{Timestamp:O} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

// ── JWT key validation (fail fast on startup) ────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
if (jwtKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key is missing or too short (minimum 32 characters). " +
        "In development run: dotnet user-secrets set \"Jwt:Key\" \"<32+ char random key>\" " +
        "In production set environment variable: JWT__KEY=<key>");

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddValidatorsFromAssembly(typeof(AssemblyMarker).Assembly);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(AuctionPlatformGe.Infrastructure.DependencyInjection).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AuctionProfile>());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero   // don't allow extra grace time on expiry
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
builder.Services.AddSignalR();
builder.Services.AddHostedService<AuctionEndingService>();

// ── Rate limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(opts =>
{
    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    opts.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Too many requests. Please slow down." }, ct);
    };

    // 5 requests/minute per IP — for login, register, OTP
    opts.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                PermitLimit = 5,
                QueueLimit = 0
            }));

    // 10 uploads/minute per authenticated user
    opts.AddPolicy("upload", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            _ => new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                PermitLimit = 10,
                QueueLimit = 0
            }));
});

// ── CORS ──────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<BidHub>("/hubs/bid");
app.MapHealthChecks("/health");

// ── Seed default admin on first boot ─────────────────────────────────────────
await EnsureAdminAsync(app);

app.Run();

static async Task EnsureAdminAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db     = scope.ServiceProvider.GetRequiredService<AuctionPlatformGe.Infrastructure.Data.AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    if (await db.Users.AnyAsync(u => u.Role == "Admin")) return;

    var email    = config["Admin:Email"]    ?? "admin@auctionplatformge.com";
    var password = config["Admin:Password"] ?? "Admin123!";
    var username = config["Admin:Username"] ?? "admin";

    var admin = new AuctionPlatformGe.Core.Entities.AppUser
    {
        Username     = username,
        Email        = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        Role         = "Admin"
    };

    db.Users.Add(admin);
    db.Wallets.Add(new AuctionPlatformGe.Core.Entities.Wallet { UserId = admin.Id, BidBalance = 0 });
    await db.SaveChangesAsync();

    logger.LogWarning(
        "Admin account created — Email: {Email} | Password: {Password} | Change this in production!",
        email, password);
}
