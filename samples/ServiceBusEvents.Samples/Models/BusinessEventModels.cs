using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceBusEvents.Samples.Models;

/// <summary>
/// Custom DateTime converter for D365 business events
/// Handles DateTime in ticks, ISO 8601, or string format
/// </summary>
public class D365DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    return DateTime.MinValue;
                }

                // Handle D365 /Date(ticks)/ format
                if (stringValue.StartsWith("/Date(") && stringValue.EndsWith(")/"))
                {
                    var ticksString = stringValue.Substring(6, stringValue.Length - 8);
                    if (long.TryParse(ticksString, out var milliseconds))
                    {
                        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
                    }
                }

                // Try standard DateTime parsing
                if (DateTime.TryParse(stringValue, out var dateTime))
                {
                    return dateTime;
                }

                throw new JsonException($"Unable to parse DateTime from string: {stringValue}");

            case JsonTokenType.Number:
                // D365 sends numeric timestamps as Unix milliseconds
                var ticks = reader.GetInt64();
                try
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(ticks).DateTime;
                }
                catch
                {
                    return DateTime.MinValue;
                }

            default:
                throw new JsonException($"Unexpected token type for DateTime: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O"));
    }
}

/// <summary>
/// Custom nullable DateTime converter for D365 business events
/// </summary>
public class D365NullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    return null;
                }

                // Handle D365 /Date(ticks)/ format
                if (stringValue.StartsWith("/Date(") && stringValue.EndsWith(")/"))
                {
                    var ticksString = stringValue.Substring(6, stringValue.Length - 8);
                    if (long.TryParse(ticksString, out var milliseconds))
                    {
                        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
                    }
                }

                if (DateTime.TryParse(stringValue, out var dateTime))
                {
                    return dateTime;
                }
                return null;

            case JsonTokenType.Number:
                // D365 sends numeric timestamps as Unix milliseconds — consistent with D365DateTimeConverter
                var ms = reader.GetInt64();
                try
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(ms).DateTime;
                }
                catch
                {
                    return null;
                }

            default:
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString("O"));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

/// <summary>
/// D365 Business Event envelope
/// </summary>
public class BusinessEventEnvelope
{
    [JsonPropertyName("BusinessEventId")]
    public string? BusinessEventId { get; set; }

    [JsonPropertyName("BusinessEventLegalEntity")]
    public string? BusinessEventLegalEntity { get; set; }

    [JsonPropertyName("ContextRecordSubject")]
    public string? ContextRecordSubject { get; set; }

    [JsonPropertyName("ControlNumber")]
    public long ControlNumber { get; set; }

    [JsonPropertyName("EventId")]
    public string? EventId { get; set; }

    [JsonPropertyName("EventTime")]
    [JsonConverter(typeof(D365DateTimeConverter))]
    public DateTime EventTime { get; set; }

    [JsonPropertyName("EventTimeIso8601")]
    public string? EventTimeIso8601 { get; set; }

    [JsonPropertyName("InitiatingUserAADObjectId")]
    public string? InitiatingUserAADObjectId { get; set; }

    [JsonPropertyName("MajorVersion")]
    public int MajorVersion { get; set; }

    [JsonPropertyName("MinorVersion")]
    public int MinorVersion { get; set; }

    [JsonPropertyName("ParentContextRecordSubjects")]
    public List<string> ParentContextRecordSubjects { get; set; } = new();

    [JsonPropertyName("ProductionOrderNumber")]
    public string? ProductionOrderNumber { get; set; }

    [JsonPropertyName("Resource")]
    public string? Resource { get; set; }

    [JsonPropertyName("LegalEntity")]
    public string? LegalEntity { get; set; }

    [JsonPropertyName("BusinessEvent")]
    public string? BusinessEvent { get; set; }
}

/// <summary>
/// Base class for TSI MES business events, containing the envelope fields common to all TSI events.
/// </summary>
public abstract class TSIBusinessEventBase
{
    [JsonPropertyName("BusinessEventId")]
    public string? BusinessEventId { get; set; }

    [JsonPropertyName("BusinessEventLegalEntity")]
    public string? BusinessEventLegalEntity { get; set; }

    [JsonPropertyName("ContextRecordSubject")]
    public string? ContextRecordSubject { get; set; }

    [JsonPropertyName("ControlNumber")]
    public long ControlNumber { get; set; }

    [JsonPropertyName("EventId")]
    public string? EventId { get; set; }

    [JsonPropertyName("EventTime")]
    [JsonConverter(typeof(D365DateTimeConverter))]
    public DateTime EventTime { get; set; }

    [JsonPropertyName("EventTimeIso8601")]
    public string? EventTimeIso8601 { get; set; }

    [JsonPropertyName("InitiatingUserAADObjectId")]
    public string? InitiatingUserAADObjectId { get; set; }

    [JsonPropertyName("MajorVersion")]
    public int MajorVersion { get; set; }

    [JsonPropertyName("MinorVersion")]
    public int MinorVersion { get; set; }

    [JsonPropertyName("ParentContextRecordSubjects")]
    public List<string> ParentContextRecordSubjects { get; set; } = new();

    [JsonPropertyName("ProductionOrderNumber")]
    public string? ProductionOrderNumber { get; set; }

    [JsonPropertyName("Resource")]
    public string? Resource { get; set; }
}

/// <summary>
/// Fired when a production order has been finalised/scheduled and is ready for the MES to pick up.
/// The MES should fetch the full order data via OData on receipt of this event.
/// </summary>
public class TSIProductionOrderReleasedToMESBusinessEvent : TSIBusinessEventBase
{
    /// <summary>The D365 BusinessEventId value used to identify this event type.</summary>
    public const string EventTypeId = "TSIProductionOrderReleasedToMESBusinessEvent";
}

/// <summary>
/// Fired when a previously released production order has been updated (e.g. quantity or schedule change)
/// and the MES should refresh its local copy of the order data.
/// </summary>
public class TSIProductionOrderUpdatedMESEvent : TSIBusinessEventBase
{
    /// <summary>The D365 BusinessEventId value used to identify this event type.</summary>
    public const string EventTypeId = "TSIProductionOrderUpdatedMESEvent";
}

/// <summary>
/// Production order released business event
/// </summary>
public class ProductionOrderReleasedEvent
{
    [JsonPropertyName("ProductionOrderNumber")]
    public string? ProductionOrderNumber { get; set; }

    [JsonPropertyName("ProductionOrderReleaseDate")]
    [JsonConverter(typeof(D365NullableDateTimeConverter))]
    public DateTime? ProductionOrderReleaseDate { get; set; }

    [JsonPropertyName("ProductionOrderType")]
    public string? ProductionOrderType { get; set; }

    [JsonPropertyName("ItemNumber")]
    public string? ItemNumber { get; set; }

    [JsonPropertyName("ProductionSiteId")]
    public string? ProductionSiteId { get; set; }

    [JsonPropertyName("ProductionWarehouseId")]
    public string? ProductionWarehouseId { get; set; }

    [JsonPropertyName("ScheduledStartDate")]
    [JsonConverter(typeof(D365NullableDateTimeConverter))]
    public DateTime? ScheduledStartDate { get; set; }

    [JsonPropertyName("ScheduledEndDate")]
    [JsonConverter(typeof(D365NullableDateTimeConverter))]
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
