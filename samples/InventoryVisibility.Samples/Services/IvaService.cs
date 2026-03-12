using System.Net.Http.Headers;
using System.Net.Http.Json;
using D365.Auth.Models;
using D365.Auth.Providers;
using InventoryVisibility.Samples.Models;
using Microsoft.Extensions.Logging;

namespace InventoryVisibility.Samples.Services;

/// <summary>
/// Service for interacting with Inventory Visibility Add-in APIs
/// </summary>
public class IvaService
{
    private readonly HttpClient _httpClient;
    private readonly IvaTokenProvider _tokenProvider;
    private readonly IvaConfig _ivaConfig;
    private readonly D365Config _d365Config;
    private readonly ILogger<IvaService> _logger;

    public IvaService(
        HttpClient httpClient,
        IvaTokenProvider tokenProvider,
        IvaConfig ivaConfig,
        D365Config d365Config,
        ILogger<IvaService> logger)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _ivaConfig = ivaConfig;
        _d365Config = d365Config;
        _logger = logger;
    }

    private async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(
        HttpMethod method,
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        var token = await _tokenProvider.GetIvaTokenAsync(cancellationToken);
        var url = $"{_ivaConfig.ServiceUrl}/api/environment/{_d365Config.EnvironmentId}{endpoint}";

        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Api-Version", "1.0");

        return request;
    }

    /// <summary>
    /// Query on-hand inventory for products. Retries on transient 500 partition-loading errors.
    /// </summary>
    public async Task<List<OnHandQueryResponse>> QueryOnHandAsync(
        OnHandQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying on-hand inventory");

        int[] retryDelaysSeconds = { 10, 20 };

        for (int attempt = 0; ; attempt++)
        {
            using var httpRequest = await CreateAuthenticatedRequestAsync(
                HttpMethod.Post,
                "/onhand/indexquery",
                cancellationToken);

            httpRequest.Content = JsonContent.Create(request);

            if (attempt == 0)
                _logger.LogInformation("Sending request to: {Url}", httpRequest.RequestUri);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if ((int)response.StatusCode == 500
                    && errorContent.Contains("Waiting for partition")
                    && attempt < retryDelaysSeconds.Length)
                {
                    _logger.LogWarning(
                        "IVA partition is loading (attempt {Attempt}), retrying in {Delay}s...",
                        attempt + 1, retryDelaysSeconds[attempt]);
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaysSeconds[attempt]), cancellationToken);
                    continue;
                }

                _logger.LogError(
                    "Query failed with status {Status}: {Error}",
                    response.StatusCode,
                    errorContent);
                response.EnsureSuccessStatusCode();
            }

            var result = await response.Content.ReadFromJsonAsync<List<OnHandQueryResponse>>(
                cancellationToken: cancellationToken) ?? new List<OnHandQueryResponse>();

            _logger.LogInformation("Retrieved {Count} inventory records", result.Count);
            return result;
        }
    }
}
