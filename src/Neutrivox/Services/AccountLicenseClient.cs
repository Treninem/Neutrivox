using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class AccountLicenseClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _http;

    public AccountLicenseClient(HttpClient? httpClient = null) => _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

    public async Task<AccountSessionInfo> RegisterAsync(string baseUrl, AccountRegisterRequest request, CancellationToken cancellationToken = default)
        => await SendAsync(HttpMethod.Post, Combine(baseUrl, "/api/v1/account/register"), request, null, cancellationToken);

    public async Task<AccountSessionInfo> LoginAsync(string baseUrl, AccountLoginRequest request, CancellationToken cancellationToken = default)
        => await SendAsync(HttpMethod.Post, Combine(baseUrl, "/api/v1/account/login"), request, null, cancellationToken);

    public async Task<AccountSessionInfo> StatusAsync(string baseUrl, string token, CancellationToken cancellationToken = default)
        => await SendAsync(HttpMethod.Get, Combine(baseUrl, "/api/v1/account/status"), null, token, cancellationToken);

    public async Task<AccountSessionInfo> RevokeDeviceAsync(string baseUrl, string token, string fingerprint, CancellationToken cancellationToken = default)
        => await SendAsync(HttpMethod.Post, Combine(baseUrl, "/api/v1/account/device/revoke"), new { Fingerprint = fingerprint }, token, cancellationToken);

    private async Task<AccountSessionInfo> SendAsync(HttpMethod method, string url, object? body, string? token, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (body is not null) request.Content = JsonContent.Create(body);
            if (!string.IsNullOrWhiteSpace(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await _http.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
                return Fail($"License server returned HTTP {(int)response.StatusCode}.");
            var parsed = JsonSerializer.Deserialize<AccountSessionInfo>(content, JsonOptions);
            return parsed ?? Fail("License server returned an invalid response.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Fail("License server request timed out.");
        }
        catch (Exception ex) when (ex is HttpRequestException or UriFormatException)
        {
            return Fail("License server is unavailable: " + ex.Message);
        }
    }

    private static string Combine(string baseUrl, string path)
    {
        if (!Uri.TryCreate(baseUrl.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new UriFormatException("License server URL must use HTTPS.");
        return new Uri(uri, path).ToString();
    }

    private static AccountSessionInfo Fail(string message) => new(false, null, null, null, null, 0, [], message);
}
