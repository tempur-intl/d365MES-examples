using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using D365.Auth.Models;
using D365.Auth.Providers;
using IntegratedEventDriven.Samples.Models;
using Microsoft.Extensions.Logging;

namespace IntegratedEventDriven.Samples.Services;

/// <summary>
/// Integrated service that consumes Service Bus events and queries OData
/// </summary>
public class IntegratedService
{
    private readonly ServiceBusConfig _serviceBusConfig;
    private readonly D365Config _d365Config;
    private readonly D365TokenProvider _tokenProvider;
    private readonly HttpClient _httpClient;
    private readonly ILogger<IntegratedService> _logger;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusReceiver _receiver;
    private const string ODataEndpoint = "/data";
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public IntegratedService(
        ServiceBusConfig serviceBusConfig,
        D365Config d365Config,
        D365TokenProvider tokenProvider,
        HttpClient httpClient,
        ILogger<IntegratedService> logger)
    {
        _serviceBusConfig = serviceBusConfig;
        _d365Config = d365Config;
        _tokenProvider = tokenProvider;
        _httpClient = httpClient;
        _logger = logger;

        _serviceBusClient = new ServiceBusClient(_serviceBusConfig.ConnectionString);
        _receiver = _serviceBusClient.CreateReceiver(
            _serviceBusConfig.TopicName,
            _serviceBusConfig.SubscriptionName,
            new ServiceBusReceiverOptions
            {
                PrefetchCount = _serviceBusConfig.PrefetchCount,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

        _logger.LogInformation("Connected to Service Bus topic: {Topic}, subscription: {Subscription}",
            _serviceBusConfig.TopicName, _serviceBusConfig.SubscriptionName);
    }

    /// <summary>
    /// Process Service Bus messages and query related production order data
    /// </summary>
    public async Task ProcessEventsAsync(int maxMessages = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Receiving up to {MaxMessages} messages from Service Bus...", maxMessages);

        var messages = await _receiver.ReceiveMessagesAsync(
            maxMessages,
            TimeSpan.FromSeconds(_serviceBusConfig.MaxWaitTimeSeconds),
            cancellationToken);

        _logger.LogInformation("Received {Count} messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                _logger.LogInformation("\n=== Processing Message {MessageId} ===", message.MessageId);

                // Step 1: Parse the business event
                var businessEvent = ParseBusinessEvent(message);
                if (businessEvent == null)
                {
                    _logger.LogWarning("Failed to parse business event, skipping message");
                    await _receiver.AbandonMessageAsync(message, cancellationToken: cancellationToken);
                    continue;
                }

                _logger.LogInformation("Business Event Received:");
                var eventJson = JsonSerializer.Serialize(businessEvent, _jsonOptions);
                Console.WriteLine(eventJson);

                // Step 2: Query production order details using OData
                var productionOrder = await GetProductionOrderAsync(
                    businessEvent.ProductionOrderNumber!,
                    cancellationToken);

                if (productionOrder != null)
                {
                    _logger.LogInformation("Production Order Details:");
                    var productionOrderJson = JsonSerializer.Serialize(productionOrder, _jsonOptions);
                    Console.WriteLine(productionOrderJson);

                    // Step 3: Query BOM lines
                    var bomLines = await GetBomLinesAsync(
                        businessEvent.ProductionOrderNumber!,
                        cancellationToken);

                    _logger.LogInformation("\nBOM Lines ({Count} materials):", bomLines.Count);
                    var bomLinesJson = JsonSerializer.Serialize(bomLines, _jsonOptions);
                    Console.WriteLine(bomLinesJson);
                }
                else
                {
                    _logger.LogWarning("Production order {OrderNumber} not found in D365",
                        businessEvent.ProductionOrderNumber);
                }

                // Step 4: Complete the message
                await _receiver.CompleteMessageAsync(message, cancellationToken);
                _logger.LogInformation("✓ Message processed successfully\n");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);

                // Handle failed messages
                if (message.DeliveryCount >= _serviceBusConfig.MaxDeliveryCount)
                {
                    await _receiver.DeadLetterMessageAsync(
                        message,
                        new Dictionary<string, object>
                        {
                            { "DeadLetterReason", "ProcessingFailed" },
                            { "DeadLetterErrorDescription", ex.Message }
                        },
                        cancellationToken);
                    _logger.LogError("Message moved to Dead Letter Queue after {Count} attempts",
                        message.DeliveryCount);
                }
                else
                {
                    await _receiver.AbandonMessageAsync(message, cancellationToken: cancellationToken);
                    _logger.LogWarning("Message will be retried (attempt {Count})", message.DeliveryCount);
                }
            }
        }
    }

    private BusinessEventEnvelope? ParseBusinessEvent(ServiceBusReceivedMessage message)
    {
        try
        {
            var bodyJson = message.Body.ToString();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<BusinessEventEnvelope>(bodyJson, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize business event");
            return null;
        }
    }

    private async Task<ProductionOrder?> GetProductionOrderAsync(
        string productionOrderNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await _tokenProvider.GetD365TokenAsync(cancellationToken);

            var filter = $"ProductionOrderNumber eq '{productionOrderNumber}' and dataAreaId eq '{_d365Config.OrganizationId}'";
            var url = $"{_d365Config.BaseUrl}{ODataEndpoint}/ProductionOrderHeaders?$filter={Uri.EscapeDataString(filter)}&$top=1";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OData query failed: {Error}", errorContent);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ODataResponse<ProductionOrder>>(
                cancellationToken: cancellationToken);

            return result?.Value.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying production order");
            return null;
        }
    }

    private async Task<List<BomLine>> GetBomLinesAsync(
        string productionOrderNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await _tokenProvider.GetD365TokenAsync(cancellationToken);

            var filter = $"ProductionOrderNumber eq '{productionOrderNumber}' and dataAreaId eq '{_d365Config.OrganizationId}'";
            var url = $"{_d365Config.BaseUrl}{ODataEndpoint}/ProductionOrderBillOfMaterialLines?$filter={Uri.EscapeDataString(filter)}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new List<BomLine>();
            }

            var result = await response.Content.ReadFromJsonAsync<ODataResponse<BomLine>>(
                cancellationToken: cancellationToken);

            return result?.Value ?? new List<BomLine>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying BOM lines");
            return new List<BomLine>();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _receiver.DisposeAsync();
        await _serviceBusClient.DisposeAsync();
    }
}
