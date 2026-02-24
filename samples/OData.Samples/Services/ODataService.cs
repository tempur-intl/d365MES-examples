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
        string? expand = null,
        int? top = null,
        CancellationToken cancellationToken = default)
    {
        var token = await _tokenProvider.GetD365TokenAsync(cancellationToken);

        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(filter))
            queryParams.Add($"$filter={Uri.EscapeDataString(filter)}");
        if (!string.IsNullOrWhiteSpace(select))
            queryParams.Add($"$select={select}");
        if (!string.IsNullOrWhiteSpace(expand))
            queryParams.Add($"$expand={expand}");
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
    /// Get TSI items
    /// </summary>
    public async Task<List<TSI_Item>> GetTsiItemsAsync(
        string? itemId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying TSI items");

        var filter = itemId != null
            ? $"ItemId eq '{itemId}'"
            : null;

        using var request = await CreateAuthenticatedRequestAsync(
            HttpMethod.Get,
            "TSI_Items",
            filter: filter,
            top: itemId != null ? 1 : null,
            cancellationToken: cancellationToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to query TSI items: {Error}", errorContent);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<ODataResponse<TSI_Item>>(
            cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} TSI items", result?.Value.Count ?? 0);
        return result?.Value ?? new List<TSI_Item>();
    }

    /// <summary>
    /// Get TSI production BOM lines
    /// </summary>
    public async Task<List<TSI_ProdBOMLine>> GetTsiProdBomLinesAsync(
        string? prodId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying TSI production BOM lines");

        var filter = prodId != null
            ? $"ProdId eq '{prodId}'"
            : null;

        using var request = await CreateAuthenticatedRequestAsync(
            HttpMethod.Get,
            "TSI_ProdBOMLines",
            filter: filter,
            cancellationToken: cancellationToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to query TSI production BOM lines: {Error}", errorContent);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<ODataResponse<TSI_ProdBOMLine>>(
            cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} TSI production BOM lines", result?.Value.Count ?? 0);
        return result?.Value ?? new List<TSI_ProdBOMLine>();
    }

    /// <summary>
    /// Get TSI labels
    /// </summary>
    public async Task<List<TSI_Label>> GetTsiLabelsAsync(
        string? prodId = null,
        string? udiUnit = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying TSI labels");

        // Build filter dynamically
        var filterParts = new List<string> { $"dataAreaId eq '{_config.OrganizationId}'" };

        if (!string.IsNullOrWhiteSpace(prodId))
        {
            filterParts.Add($"ProdId eq '{prodId}'");
        }

        if (!string.IsNullOrWhiteSpace(udiUnit))
        {
            filterParts.Add($"(UDIUnit eq '{udiUnit}' or HasUDI eq 0)");
        }

        var filter = string.Join(" and ", filterParts);

        using var request = await CreateAuthenticatedRequestAsync(
            HttpMethod.Get,
            "TSI_Labels",
            filter: filter,
            select: "ProdId,ItemId,InventDimId,EntityKey,LabelSalesOrder,LabelEAN_Code,LabelMadeIn,LabelDateWeek,UDIUnit,HasUDI",
            expand: "Logos",
            cancellationToken: cancellationToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to query TSI labels: {Error}", errorContent);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<ODataResponse<TSI_Label>>(
            cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} TSI labels", result?.Value.Count ?? 0);
        return result?.Value ?? new List<TSI_Label>();
    }

    /// <summary>
    /// Get TSI jobs
    /// </summary>
    public async Task<List<TSI_Job>> GetTsiJobsAsync(
        string? prodId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying TSI jobs");

        var filter = prodId != null
            ? $"ProdId eq '{prodId}' and dataAreaId eq '{_config.OrganizationId}'"
            : $"dataAreaId eq '{_config.OrganizationId}'";

        using var request = await CreateAuthenticatedRequestAsync(
            HttpMethod.Get,
            "TSI_Jobs",
            filter: filter,
            cancellationToken: cancellationToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to query TSI jobs: {Error}", errorContent);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<ODataResponse<TSI_Job>>(
            cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} TSI jobs", result?.Value.Count ?? 0);
        return result?.Value ?? new List<TSI_Job>();
    }

    /// <summary>
    /// Get warehouse work lines
    /// </summary>
    public async Task<List<WarehouseWorkLines>> GetWarehouseWorkLinesAsync(
        string? filter = null,
        int? top = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying warehouse work lines");

        // Add dataAreaId filter
        var fullFilter = string.IsNullOrWhiteSpace(filter)
            ? $"dataAreaId eq '{_config.OrganizationId}'"
            : $"{filter} and dataAreaId eq '{_config.OrganizationId}'";

        using var request = await CreateAuthenticatedRequestAsync(
            HttpMethod.Get,
            "WarehouseWorkLines",
            filter: fullFilter,
            top: top,
            cancellationToken: cancellationToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to query warehouse work lines: {Error}", errorContent);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<ODataResponse<WarehouseWorkLines>>(
            cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} warehouse work lines", result?.Value.Count ?? 0);
        return result?.Value ?? new List<WarehouseWorkLines>();
    }

    /// <summary>
    /// Get item batches
    /// </summary>
    public async Task<List<ItemBatches>> GetItemBatchesAsync(
        string? filter = null,
        int? top = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying item batches");

        // Add dataAreaId filter
        var fullFilter = string.IsNullOrWhiteSpace(filter)
            ? $"dataAreaId eq '{_config.OrganizationId}'"
            : $"{filter} and dataAreaId eq '{_config.OrganizationId}'";

        using var request = await CreateAuthenticatedRequestAsync(
            HttpMethod.Get,
            "ItemBatches",
            filter: fullFilter,
            top: top,
            cancellationToken: cancellationToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to query item batches: {Error}", errorContent);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<ODataResponse<ItemBatches>>(
            cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} item batches", result?.Value.Count ?? 0);
        return result?.Value ?? new List<ItemBatches>();
    }
}
