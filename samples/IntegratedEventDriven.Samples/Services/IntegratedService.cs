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

        _serviceBusClient = new ServiceBusClient(_serviceBusConfig.ConnectionString,
            new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            });
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
        var messages = await _receiver.ReceiveMessagesAsync(
            maxMessages,
            TimeSpan.FromSeconds(_serviceBusConfig.MaxWaitTimeSeconds),
            cancellationToken);

        int mesMessageCount = 0;

        foreach (var message in messages)
        {
            try
            {
                // Step 1: Parse the business event
                var businessEvent = ParseBusinessEvent(message);
                if (businessEvent == null)
                {
                    _logger.LogWarning("Failed to parse business event, skipping message");
                    await _receiver.AbandonMessageAsync(message, cancellationToken: cancellationToken);
                    continue;
                }

                // Only process TSI Production Order Released to MES events
                if (businessEvent.BusinessEventId != "TSIProductionOrderReleasedToMESBusinessEvent")
                {
                    await _receiver.CompleteMessageAsync(message, cancellationToken);
                    continue;
                }

                mesMessageCount++;

                _logger.LogInformation("\n=== Processing Message {MessageId} ===", message.MessageId);
                _logger.LogInformation("Business Event Received:");
                var eventJson = JsonSerializer.Serialize(businessEvent, _jsonOptions);
                Console.WriteLine(eventJson);

                // Log specific event details
                _logger.LogInformation("Event Type: {EventType}", businessEvent.BusinessEventId);
                _logger.LogInformation("Production Order: {OrderNumber}", businessEvent.ProductionOrderNumber);
                _logger.LogInformation("Resource: {Resource}", businessEvent.Resource);

                // Step 2: Query job details using OData (ProdId = ProductionOrderNumber in D365)
                var prodId = businessEvent.ProductionOrderNumber!;
                var jobs = await GetJobsAsync(prodId, cancellationToken);

                if (jobs.Any())
                {
                    _logger.LogInformation("Job Details ({Count} jobs):", jobs.Count);
                    var jobsJson = JsonSerializer.Serialize(jobs, _jsonOptions);
                    Console.WriteLine(jobsJson);

                    // Step 3: Query BOM lines
                    var bomLines = await GetBomLinesAsync(prodId, cancellationToken);

                    _logger.LogInformation("\nBOM Lines ({Count} materials):", bomLines.Count);
                    var bomLinesJson = JsonSerializer.Serialize(bomLines, _jsonOptions);
                    Console.WriteLine(bomLinesJson);
                }
                else
                {
                    _logger.LogWarning("No jobs found for production order {OrderNumber}",
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

        _logger.LogInformation("Processed {Count} TSIProductionOrderReleasedToMESBusinessEvent messages", mesMessageCount);
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

    private async Task<List<TSI_Job>> GetJobsAsync(
        string prodId,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await _tokenProvider.GetD365TokenAsync(cancellationToken);

            var filter = $"ProdId eq '{prodId}' and dataAreaId eq '{_d365Config.OrganizationId}'";
            var url = $"{_d365Config.BaseUrl}{ODataEndpoint}/TSI_Jobs?$filter={Uri.EscapeDataString(filter)}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OData query failed: {Error}", errorContent);
                return new List<TSI_Job>();
            }

            var result = await response.Content.ReadFromJsonAsync<ODataResponse<TSI_Job>>(
                cancellationToken: cancellationToken);

            return result?.Value ?? new List<TSI_Job>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying jobs");
            return new List<TSI_Job>();
        }
    }

    private async Task<List<TSI_ProdBOMLine>> GetBomLinesAsync(
        string prodId,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await _tokenProvider.GetD365TokenAsync(cancellationToken);

            var filter = $"ProdId eq '{prodId}' and dataAreaId eq '{_d365Config.OrganizationId}'";
            var url = $"{_d365Config.BaseUrl}{ODataEndpoint}/TSI_ProdBOMLines?$filter={Uri.EscapeDataString(filter)}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OData query failed: {Error}", errorContent);
                return new List<TSI_ProdBOMLine>();
            }

            var result = await response.Content.ReadFromJsonAsync<ODataResponse<TSI_ProdBOMLine>>(
                cancellationToken: cancellationToken);

            return result?.Value ?? new List<TSI_ProdBOMLine>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying BOM lines");
            return new List<TSI_ProdBOMLine>();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _receiver.DisposeAsync();
        await _serviceBusClient.DisposeAsync();
    }
}
