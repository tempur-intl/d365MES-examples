using System.Net.Http.Headers;
using System.Net.Http.Json;
using D365.Auth.Models;
using D365.Auth.Providers;
using OData.Samples.Models;
using Microsoft.Extensions.Logging;

namespace OData.Samples.Services;

/// <summary>
/// Service for querying D365 OData endpoints
/// </summary>
public class ODataService
{
    private readonly HttpClient _httpClient;
    private readonly D365TokenProvider _tokenProvider;
    private readonly D365Config _config;
    private readonly ILogger<ODataService> _logger;
    private const string ODataEndpoint = "/data";

    public ODataService(
        HttpClient httpClient,
        D365TokenProvider tokenProvider,
        D365Config config,
        ILogger<ODataService> logger)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _config = config;
        _logger = logger;
    }

    private async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(
        HttpMethod method,
        string entityPath,
        string? filter = null,
        string? select = null,
        int? top = null,
        CancellationToken cancellationToken = default)
    {
        var token = await _tokenProvider.GetD365TokenAsync(cancellationToken);

        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(filter))
            queryParams.Add($"$filter={Uri.EscapeDataString(filter)}");
        if (!string.IsNullOrWhiteSpace(select))
            queryParams.Add($"$select={select}");
        if (top.HasValue)
            queryParams.Add($"$top={top.Value}");

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var url = $"{_config.BaseUrl}{ODataEndpoint}/{entityPath}{query}";

        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return request;
    }

    /// <summary>
    /// Query production orders
    /// </summary>
    public async Task<List<ProductionOrder>> GetProductionOrdersAsync(
        string? filter = null,
        int? top = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying production orders");

        // Add dataAreaId filter
        var fullFilter = string.IsNullOrWhiteSpace(filter)
            ? $"dataAreaId eq '{_config.OrganizationId}'"
            : $"{filter} and dataAreaId eq '{_config.OrganizationId}'";

        using var request = await CreateAuthenticatedRequestAsync(
            HttpMethod.Get,
            "ProductionOrderHeaders",
            filter: fullFilter,
            top: top,
            cancellationToken: cancellationToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to query production orders: {Error}", errorContent);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<ODataResponse<ProductionOrder>>(
            cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} production orders", result?.Value.Count ?? 0);
        return result?.Value ?? new List<ProductionOrder>();
    }

    /// <summary>
    /// Get BOM lines for a specific BOM
    /// </summary>
    public async Task<List<BomLine>> GetBomLinesAsync(
        string productionOrderNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying BOM lines for production order: {OrderNumber}", productionOrderNumber);

        var filter = $"dataAreaId eq '{_config.OrganizationId}' and ProductionOrderNumber eq '{productionOrderNumber}'";

        using var request = await CreateAuthenticatedRequestAsync(
            HttpMethod.Get,
            "ProductionOrderBillOfMaterialLines",
            filter: filter,
            cancellationToken: cancellationToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to query BOM lines: {Error}", errorContent);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<ODataResponse<BomLine>>(
            cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} BOM lines", result?.Value?.Count ?? 0);
        return result?.Value ?? new List<BomLine>();
    }

    /// <summary>
    /// Get route operations for a specific route
    /// </summary>
    public async Task<List<RouteOperation>> GetRouteOperationsAsync(
        string productionOrderNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying route operations for production order: {OrderNumber}", productionOrderNumber);

        var filter = $"dataAreaId eq '{_config.OrganizationId}' and ProductionOrderNumber eq '{productionOrderNumber}'";

        using var request = await CreateAuthenticatedRequestAsync(
            HttpMethod.Get,
            "ProductionOrderRouteOperations",
            filter: filter,
            cancellationToken: cancellationToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to query route operations: {Error}", errorContent);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<ODataResponse<RouteOperation>>(
            cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} route operations", result?.Value.Count ?? 0);
        return result?.Value ?? new List<RouteOperation>();
    }

    /// <summary>
    /// Get released product information
    /// </summary>
    public async Task<ReleasedProduct?> GetProductAsync(
        string productNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying product: {ProductNumber}", productNumber);

        var filter = $"ProductNumber eq '{productNumber}' and dataAreaId eq '{_config.OrganizationId}'";

        using var request = await CreateAuthenticatedRequestAsync(
            HttpMethod.Get,
            "ReleasedProductsV2",
            filter: filter,
            top: 1,
            cancellationToken: cancellationToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to query product: {Error}", errorContent);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<ODataResponse<ReleasedProduct>>(
            cancellationToken: cancellationToken);

        var product = result?.Value.FirstOrDefault();
        if (product != null)
        {
            _logger.LogInformation("Found product: {ProductName}", product.ProductName);
        }

        return product;
    }

}
