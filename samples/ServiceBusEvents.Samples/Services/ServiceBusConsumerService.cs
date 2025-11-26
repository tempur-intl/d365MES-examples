using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using ServiceBusEvents.Samples.Models;

namespace ServiceBusEvents.Samples.Services;

/// <summary>
/// Service for consuming D365 business events from Azure Service Bus
/// </summary>
public class ServiceBusConsumerService
{
    private readonly ServiceBusConfig _config;
    private readonly ILogger<ServiceBusConsumerService> _logger;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusReceiver _receiver;

    public ServiceBusConsumerService(
        ServiceBusConfig config,
        ILogger<ServiceBusConsumerService> logger)
    {
        _config = config;
        _logger = logger;

        // Create Service Bus client
        _client = new ServiceBusClient(_config.ConnectionString);

        // Create receiver based on entity type
        if (_config.EntityType.Equals("Topic", StringComparison.OrdinalIgnoreCase))
        {
            _receiver = _client.CreateReceiver(
                _config.TopicName,
                _config.SubscriptionName,
                new ServiceBusReceiverOptions
                {
                    PrefetchCount = _config.PrefetchCount,
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                });
            _logger.LogInformation("Connected to topic: {Topic}, subscription: {Subscription}",
                _config.TopicName, _config.SubscriptionName);
        }
        else
        {
            _receiver = _client.CreateReceiver(
                _config.QueueName,
                new ServiceBusReceiverOptions
                {
                    PrefetchCount = _config.PrefetchCount,
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                });
            _logger.LogInformation("Connected to queue: {Queue}", _config.QueueName);
        }
    }

    /// <summary>
    /// Receive and process messages once (poll mode)
    /// </summary>
    public async Task<List<ProcessedMessage>> ReceiveMessagesAsync(
        int maxMessages = 10,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ProcessedMessage>();
        var messages = await _receiver.ReceiveMessagesAsync(
            maxMessages,
            TimeSpan.FromSeconds(_config.MaxWaitTimeSeconds),
            cancellationToken);

        _logger.LogInformation("Received {Count} messages from Service Bus", messages.Count);

        foreach (var message in messages)
        {
            var result = await ProcessMessageAsync(message, cancellationToken);
            results.Add(result);

            if (result.Success)
            {
                await _receiver.CompleteMessageAsync(message, cancellationToken);
            }
            else
            {
                // Let it retry or go to DLQ based on MaxDeliveryCount
                if (message.DeliveryCount >= _config.MaxDeliveryCount)
                {
                    _logger.LogError("Message {MessageId} exceeded max delivery count, moving to DLQ",
                        message.MessageId);
                    await _receiver.DeadLetterMessageAsync(
                        message,
                        new Dictionary<string, object>
                        {
                            { "DeadLetterReason", "MaxDeliveryCountExceeded" },
                            { "DeadLetterErrorDescription", result.ErrorMessage ?? "Unknown error" }
                        },
                        cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Message {MessageId} will be retried (attempt {Attempt})",
                        message.MessageId, message.DeliveryCount);
                    await _receiver.AbandonMessageAsync(message, cancellationToken: cancellationToken);
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Start continuous message processing (background mode)
    /// </summary>
    public async Task StartContinuousProcessingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting continuous message processing. Press Ctrl+C to stop.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var messages = await _receiver.ReceiveMessagesAsync(
                    _config.PrefetchCount,
                    TimeSpan.FromSeconds(_config.MaxWaitTimeSeconds),
                    cancellationToken);

                if (messages.Count == 0)
                {
                    _logger.LogDebug("No messages received, waiting...");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    continue;
                }

                _logger.LogInformation("Received {Count} messages", messages.Count);

                foreach (var message in messages)
                {
                    var result = await ProcessMessageAsync(message, cancellationToken);

                    if (result.Success)
                    {
                        await _receiver.CompleteMessageAsync(message, cancellationToken);
                    }
                    else
                    {
                        if (message.DeliveryCount >= _config.MaxDeliveryCount)
                        {
                            await _receiver.DeadLetterMessageAsync(
                                message,
                                new Dictionary<string, object>
                                {
                                    { "DeadLetterReason", "ProcessingFailed" },
                                    { "DeadLetterErrorDescription", result.ErrorMessage ?? "Unknown error" }
                                },
                                cancellationToken);
                        }
                        else
                        {
                            await _receiver.AbandonMessageAsync(message, cancellationToken: cancellationToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Continuous processing cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in continuous processing loop");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Check dead letter queue for failed messages
    /// </summary>
    public async Task<List<ProcessedMessage>> CheckDeadLetterQueueAsync(
        int maxMessages = 10,
        CancellationToken cancellationToken = default)
    {
        ServiceBusReceiver dlqReceiver;

        if (_config.EntityType.Equals("Topic", StringComparison.OrdinalIgnoreCase))
        {
            dlqReceiver = _client.CreateReceiver(
                _config.TopicName,
                _config.SubscriptionName,
                new ServiceBusReceiverOptions
                {
                    SubQueue = SubQueue.DeadLetter,
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                });
        }
        else
        {
            dlqReceiver = _client.CreateReceiver(
                _config.QueueName,
                new ServiceBusReceiverOptions
                {
                    SubQueue = SubQueue.DeadLetter,
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                });
        }

        var results = new List<ProcessedMessage>();
        var messages = await dlqReceiver.ReceiveMessagesAsync(
            maxMessages,
            TimeSpan.FromSeconds(_config.MaxWaitTimeSeconds),
            cancellationToken);

        _logger.LogInformation("Found {Count} messages in dead letter queue", messages.Count);

        foreach (var message in messages)
        {
            var result = await ProcessMessageAsync(message, cancellationToken, isDlq: true);
            results.Add(result);
        }

        await dlqReceiver.DisposeAsync();
        return results;
    }

    private Task<ProcessedMessage> ProcessMessageAsync(
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken,
        bool isDlq = false)
    {
        var result = new ProcessedMessage
        {
            MessageId = message.MessageId,
            DeliveryCount = message.DeliveryCount,
            EnqueuedTime = message.EnqueuedTime.DateTime,
            Success = false
        };

        try
        {
            var bodyJson = message.Body.ToString();
            _logger.LogDebug("Processing message {MessageId}, delivery count: {Count}",
                message.MessageId, message.DeliveryCount);

            // Parse the D365 business event envelope
            var envelope = JsonSerializer.Deserialize<BusinessEventEnvelope>(bodyJson);

            if (envelope == null)
            {
                throw new InvalidOperationException("Failed to deserialize business event envelope");
            }

            result.EventId = envelope.EventId;
            result.EventTime = envelope.EventTime;
            result.LegalEntity = envelope.LegalEntity;

            // Parse the inner business event based on EventId
            if (envelope.BusinessEvent != null)
            {
                result.EventType = envelope.EventId;
                result.EventData = ParseBusinessEvent(envelope.EventId, envelope.BusinessEvent);
            }

            result.Success = true;
            _logger.LogInformation("Successfully processed message {MessageId}, Event: {EventId}",
                message.MessageId, envelope.EventId);

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to process message {MessageId}", message.MessageId);
            return Task.FromResult(result);
        }
    }

    private object? ParseBusinessEvent(string? eventId, string businessEventJson)
    {
        return eventId switch
        {
            "ProductionOrderReleasedBusinessEvent" =>
                JsonSerializer.Deserialize<ProductionOrderReleasedEvent>(businessEventJson),
            "ProductionOrderStatusChangedBusinessEvent" =>
                JsonSerializer.Deserialize<ProductionOrderStatusChangedEvent>(businessEventJson),
            _ => JsonSerializer.Deserialize<JsonElement>(businessEventJson)
        };
    }

    public async ValueTask DisposeAsync()
    {
        await _receiver.DisposeAsync();
        await _client.DisposeAsync();
    }
}
