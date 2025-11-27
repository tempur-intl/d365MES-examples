using System.Text.Json.Serialization;

namespace ServiceBusEvents.Samples.Models;

/// <summary>
/// D365 Business Event envelope
/// </summary>
public class BusinessEventEnvelope
{
    [JsonPropertyName("BusinessEventId")]
    public string? BusinessEventId { get; set; }

    [JsonPropertyName("ControlNumber")]
    public long ControlNumber { get; set; }

    [JsonPropertyName("EventId")]
    public string? EventId { get; set; }

    [JsonPropertyName("EventTime")]
    public DateTime EventTime { get; set; }

    [JsonPropertyName("MajorVersion")]
    public int MajorVersion { get; set; }

    [JsonPropertyName("MinorVersion")]
    public int MinorVersion { get; set; }

    [JsonPropertyName("LegalEntity")]
    public string? LegalEntity { get; set; }

    [JsonPropertyName("BusinessEvent")]
    public string? BusinessEvent { get; set; }
}

/// <summary>
/// Production order released business event
/// </summary>
public class ProductionOrderReleasedEvent
{
    [JsonPropertyName("ProductionOrderNumber")]
    public string? ProductionOrderNumber { get; set; }

    [JsonPropertyName("ItemNumber")]
    public string? ItemNumber { get; set; }

    [JsonPropertyName("ProductionSiteId")]
    public string? ProductionSiteId { get; set; }

    [JsonPropertyName("ProductionWarehouseId")]
    public string? ProductionWarehouseId { get; set; }

    [JsonPropertyName("ScheduledStartDate")]
    public DateTime? ScheduledStartDate { get; set; }

    [JsonPropertyName("ScheduledEndDate")]
    public DateTime? ScheduledEndDate { get; set; }

    [JsonPropertyName("ProductionOrderStatus")]
    public string? ProductionOrderStatus { get; set; }

    [JsonPropertyName("RemainingSchedulingQuantity")]
    public decimal RemainingSchedulingQuantity { get; set; }

    [JsonPropertyName("DefaultLedgerDimensionDisplayValue")]
    public string? DefaultLedgerDimensionDisplayValue { get; set; }
}

/// <summary>
/// Service Bus configuration
/// </summary>
public class ServiceBusConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string EntityType { get; set; } = "Topic"; // Topic or Queue
    public string TopicName { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public int MaxDeliveryCount { get; set; } = 3;
    public int PrefetchCount { get; set; } = 10;
    public int MaxWaitTimeSeconds { get; set; } = 30;
}

/// <summary>
/// Processed message result
/// </summary>
public class ProcessedMessage
{
    public string? MessageId { get; set; }
    public string? EventId { get; set; }
    public string? EventType { get; set; }
    public DateTime EventTime { get; set; }
    public string? LegalEntity { get; set; }
    public object? EventData { get; set; }
    public int DeliveryCount { get; set; }
    public DateTime EnqueuedTime { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
