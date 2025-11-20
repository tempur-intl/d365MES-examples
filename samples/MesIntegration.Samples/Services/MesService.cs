using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using D365.Auth.Models;
using D365.Auth.Providers;
using MesIntegration.Samples.Models;
using Microsoft.Extensions.Logging;

namespace MesIntegration.Samples.Services;

/// <summary>
/// Service for D365 MES Integration API
/// </summary>
public class MesService
{
    private readonly HttpClient _httpClient;
    private readonly D365TokenProvider _tokenProvider;
    private readonly D365Config _config;
    private readonly ILogger<MesService> _logger;
    private const string MessageServiceEndpoint = "/api/services/SysMessageServices/SysMessageService/SendMessage";

    public MesService(
        HttpClient httpClient,
        D365TokenProvider tokenProvider,
        D365Config config,
        ILogger<MesService> logger)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _config = config;
        _logger = logger;
    }

    private async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(
        HttpMethod method,
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        var token = await _tokenProvider.GetD365TokenAsync(cancellationToken);
        var url = $"{_config.BaseUrl}{endpoint}";

        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return request;
    }

    private async Task SendMessageAsync<T>(
        string messageType,
        T messageContent,
        CancellationToken cancellationToken = default)
    {
        var messageContentJson = JsonSerializer.Serialize(messageContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null // Use exact property names
        });

        var envelope = new MesMessageEnvelope
        {
            CompanyId = _config.OrganizationId,
            MessageType = messageType,
            MessageContent = messageContentJson
        };

        _logger.LogInformation("Sending MES message: {MessageType}", messageType);
        _logger.LogDebug("Message content: {Content}", messageContentJson);

        using var request = await CreateAuthenticatedRequestAsync(
            HttpMethod.Post,
            MessageServiceEndpoint,
            cancellationToken);

        request.Content = JsonContent.Create(envelope);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Failed to send MES message. Status: {StatusCode}, Error: {Error}",
                response.StatusCode,
                errorContent);
            response.EnsureSuccessStatusCode();
        }

        _logger.LogInformation("MES message sent successfully");
    }

    /// <summary>
    /// Start a production order
    /// </summary>
    public async Task StartProductionOrderAsync(
        StartProductionOrderMessage message,
        CancellationToken cancellationToken = default)
    {
        await SendMessageAsync(
            "ProdProductionOrderStart",
            message,
            cancellationToken);
    }

    /// <summary>
    /// Report production as finished
    /// </summary>
    public async Task ReportAsFinishedAsync(
        ReportAsFinishedMessage message,
        CancellationToken cancellationToken = default)
    {
        await SendMessageAsync(
            "ProdProductionOrderReportFinished",
            message,
            cancellationToken);
    }

    /// <summary>
    /// Report material consumption (picking list)
    /// </summary>
    public async Task ReportMaterialConsumptionAsync(
        MaterialConsumptionMessage message,
        CancellationToken cancellationToken = default)
    {
        await SendMessageAsync(
            "ProdProductionOrderPickingList",
            message,
            cancellationToken);
    }

    /// <summary>
    /// Report time consumed for operations (route card)
    /// </summary>
    public async Task ReportTimeConsumptionAsync(
        RouteCardMessage message,
        CancellationToken cancellationToken = default)
    {
        await SendMessageAsync(
            "ProdProductionOrderRouteCard",
            message,
            cancellationToken);
    }

    /// <summary>
    /// End a production order
    /// </summary>
    public async Task EndProductionOrderAsync(
        EndProductionOrderMessage message,
        CancellationToken cancellationToken = default)
    {
        await SendMessageAsync(
            "ProdProductionOrderEnd",
            message,
            cancellationToken);
    }
}
