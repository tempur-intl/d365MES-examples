using System.Net.Http.Headers;
using System.Net.Http.Json;
using D365.Auth.Models;
using Microsoft.Extensions.Logging;

namespace D365.Auth.Providers;

/// <summary>
/// Provides authentication tokens for Inventory Visibility Add-in API
/// Implements the two-step authentication process:
/// 1. Acquire Azure AD token
/// 2. Exchange for IVA access token from security service
/// </summary>
public class IvaTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly AzureAdTokenProvider _azureAdTokenProvider;
    private readonly IvaConfig _ivaConfig;
    private readonly D365Config _d365Config;
    private readonly ILogger<IvaTokenProvider> _logger;
    private TokenResponse? _cachedToken;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public IvaTokenProvider(
        HttpClient httpClient,
        AzureAdTokenProvider azureAdTokenProvider,
        IvaConfig ivaConfig,
        D365Config d365Config,
        ILogger<IvaTokenProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _azureAdTokenProvider = azureAdTokenProvider ?? throw new ArgumentNullException(nameof(azureAdTokenProvider));
        _ivaConfig = ivaConfig ?? throw new ArgumentNullException(nameof(ivaConfig));
        _d365Config = d365Config ?? throw new ArgumentNullException(nameof(d365Config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateConfiguration();
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_d365Config.EnvironmentId))
            throw new ArgumentException("EnvironmentId is required", nameof(_d365Config.EnvironmentId));

        if (string.IsNullOrWhiteSpace(_ivaConfig.SecurityServiceUrl))
            throw new ArgumentException("SecurityServiceUrl is required", nameof(_ivaConfig.SecurityServiceUrl));
    }

    /// <summary>
    /// Gets an Inventory Visibility Add-in access token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>IVA access token for API calls</returns>
    public async Task<string> GetIvaTokenAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Return cached token if still valid
            if (_cachedToken != null && !_cachedToken.IsExpired)
            {
                _logger.LogDebug("Using cached IVA token");
                return _cachedToken.AccessToken;
            }

            _logger.LogInformation("Acquiring new Inventory Visibility Add-in token");

            // Step 1: Get Azure AD token with IVA scope
            var aadToken = await _azureAdTokenProvider.GetTokenAsync(
                "0cdb527f-a8d1-4bf8-9436-b352c68682b2/.default",
                cancellationToken);

            _logger.LogDebug("Azure AD token acquired, length: {Length}", aadToken.Length);

            // Step 2: Exchange Azure AD token for IVA access token
            var tokenRequest = new IvaSecurityTokenRequest
            {
                ClientAssertion = aadToken,
                Context = _d365Config.EnvironmentId
            };

            var tokenUrl = $"{_ivaConfig.SecurityServiceUrl}/token";
            _logger.LogInformation("Exchanging token at: {TokenUrl} for environment: {EnvironmentId}",
                tokenUrl, _d365Config.EnvironmentId);

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = JsonContent.Create(tokenRequest)
            };

            request.Headers.Add("Api-Version", "1.0");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            // Handle 307 redirect if necessary
            if (response.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect)
            {
                var redirectUrl = response.Headers.Location?.ToString();
                if (!string.IsNullOrWhiteSpace(redirectUrl))
                {
                    _logger.LogInformation("Following redirect to: {RedirectUrl}", redirectUrl);

                    using var redirectRequest = new HttpRequestMessage(HttpMethod.Post, redirectUrl)
                    {
                        Content = JsonContent.Create(tokenRequest)
                    };
                    redirectRequest.Headers.Add("Api-Version", "1.0");

                    response = await _httpClient.SendAsync(redirectRequest, cancellationToken);
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to acquire IVA token. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException(
                    $"Failed to acquire IVA token: {response.StatusCode}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<IvaSecurityTokenResponse>(
                cancellationToken: cancellationToken);

            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException("Invalid token response from IVA security service");
            }

            _cachedToken = new TokenResponse
            {
                AccessToken = tokenResponse.AccessToken,
                TokenType = tokenResponse.TokenType,
                ExpiresIn = tokenResponse.ExpiresIn,
                ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
            };

            _logger.LogInformation(
                "Successfully acquired IVA token (expires in {ExpiresIn} seconds)",
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
        _logger.LogDebug("IVA token cache cleared");
    }
}
