using System.Threading.RateLimiting;
using Neutrivox.LicenseServer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<LicenseServerStore>();
builder.Services.AddSingleton<AccountLicenseService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 12,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

var app = builder.Build();
app.UseHttpsRedirection();
app.UseRateLimiter();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "Neutrivox.LicenseServer" }));

app.MapPost("/api/v1/account/register", async (RegisterAccountRequest request, AccountLicenseService service, CancellationToken ct) =>
{
    var result = await service.RegisterAsync(request, ct);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
}).RequireRateLimiting("auth");

app.MapPost("/api/v1/account/login", async (LoginAccountRequest request, AccountLicenseService service, CancellationToken ct) =>
{
    var result = await service.LoginAsync(request, ct);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
}).RequireRateLimiting("auth");

app.MapGet("/api/v1/account/status", async (HttpRequest request, AccountLicenseService service, CancellationToken ct) =>
{
    var token = Bearer(request);
    if (token is null) return Results.Unauthorized();
    var result = await service.StatusAsync(token, ct);
    return result.Success ? Results.Ok(result) : Results.Unauthorized();
});

app.MapPost("/api/v1/account/device/revoke", async (HttpRequest http, RevokeDeviceRequest request, AccountLicenseService service, CancellationToken ct) =>
{
    var token = Bearer(http);
    if (token is null) return Results.Unauthorized();
    var result = await service.RevokeDeviceAsync(token, request, ct);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.Run();

static string? Bearer(HttpRequest request)
{
    var header = request.Headers.Authorization.ToString();
    const string prefix = "Bearer ";
    return header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && header.Length > prefix.Length
        ? header[prefix.Length..].Trim()
        : null;
}

public partial class Program { }
