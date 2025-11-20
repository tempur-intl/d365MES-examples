using System.Net.Http.Json;
using System.Text;
using D365.Auth.Models;
using Microsoft.Extensions.Logging;

namespace D365.Auth.Providers;

/// <summary>
/// Provides Azure AD (Microsoft Entra ID) token acquisition
/// This is the base authentication for both D365 and Inventory Visibility
/// </summary>
public class AzureAdTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly AzureAdConfig _config;
    private readonly ILogger<AzureAdTokenProvider> _logger;
    private TokenResponse? _cachedToken;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public AzureAdTokenProvider(
        HttpClient httpClient,
        AzureAdConfig config,
        ILogger<AzureAdTokenProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateConfiguration();
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_config.TenantId))
            throw new ArgumentException("TenantId is required", nameof(_config.TenantId));

        if (string.IsNullOrWhiteSpace(_config.ClientId))
            throw new ArgumentException("ClientId is required", nameof(_config.ClientId));

        if (string.IsNullOrWhiteSpace(_config.ClientSecret))
            throw new ArgumentException("ClientSecret is required", nameof(_config.ClientSecret));
    }

    /// <summary>
    /// Gets an Azure AD access token, using cache if valid
    /// </summary>
    /// <param name="scope">The OAuth scope to request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token</returns>
    public async Task<string> GetTokenAsync(
        string? scope = null,
        CancellationToken cancellationToken = default)
    {
        scope ??= _config.Scope;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Return cached token if still valid
            if (_cachedToken != null && !_cachedToken.IsExpired)
            {
                _logger.LogDebug("Using cached Azure AD token");
                return _cachedToken.AccessToken;
            }

            _logger.LogInformation("Acquiring new Azure AD token from Microsoft Entra ID");

            var tokenUrl = $"https://login.microsoftonline.com/{_config.TenantId}/oauth2/v2.0/token";

            var formData = new Dictionary<string, string>
            {
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret,
                ["grant_type"] = "client_credentials",
                ["scope"] = scope
            };

            using var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(tokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to acquire Azure AD token. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException(
                    $"Failed to acquire Azure AD token: {response.StatusCode}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<AzureAdTokenResponse>(
                cancellationToken: cancellationToken);

            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException("Invalid token response from Azure AD");
            }

            _cachedToken = new TokenResponse
            {
                AccessToken = tokenResponse.AccessToken,
                TokenType = tokenResponse.TokenType,
                ExpiresIn = tokenResponse.ExpiresIn,
                ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
            };

            _logger.LogInformation(
                "Successfully acquired Azure AD token (expires in {ExpiresIn} seconds)",
                tokenResponse.ExpiresIn);

            return _cachedToken.AccessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Clears the cached token (useful for testing or forcing refresh)
    /// </summary>
    public void ClearCache()
    {
        _cachedToken = null;
        _logger.LogDebug("Azure AD token cache cleared");
    }
}
