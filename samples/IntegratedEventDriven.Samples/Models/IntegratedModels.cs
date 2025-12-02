using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntegratedEventDriven.Samples.Models;

/// <summary>
/// Custom DateTime converter for D365 business events
/// Handles DateTime in /Date(ticks)/ format
/// </summary>
public class D365DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
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
        }

        return DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O"));
    }
}

/// <summary>
/// D365 Business Event envelope
/// </summary>
public class BusinessEventEnvelope
{
    [JsonPropertyName("BusinessEventId")]
    public string? BusinessEventId { get; set; }

    [JsonPropertyName("EventTime")]
    [JsonConverter(typeof(D365DateTimeConverter))]
    public DateTime EventTime { get; set; }

    [JsonPropertyName("LegalEntity")]
    public string? LegalEntity { get; set; }

    [JsonPropertyName("ProductionOrderNumber")]
    public string? ProductionOrderNumber { get; set; }

    [JsonPropertyName("ProductionOrderType")]
    public string? ProductionOrderType { get; set; }
}

/// <summary>
/// Service Bus configuration
/// </summary>
public class ServiceBusConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string EntityType { get; set; } = "Topic";
    public string TopicName { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
    public int MaxDeliveryCount { get; set; } = 3;
    public int PrefetchCount { get; set; } = 10;
    public int MaxWaitTimeSeconds { get; set; } = 30;
}

/// <summary>
/// OData response wrapper
/// </summary>
public class ODataResponse<T>
{
    [JsonPropertyName("value")]
    public List<T> Value { get; set; } = new();
}

/// <summary>
/// Production Order Header - Complete model matching D365
/// </summary>
public class ProductionOrder
{
    // Key fields
    [JsonPropertyName("dataAreaId")]
    public string? DataAreaId { get; set; }

    [JsonPropertyName("ProductionOrderNumber")]
    public string? ProductionOrderNumber { get; set; }

    // Product identification
    [JsonPropertyName("ItemNumber")]
    public string? ItemNumber { get; set; }

    [JsonPropertyName("ProductStyleId")]
    public string? ProductStyleId { get; set; }

    [JsonPropertyName("ProductSizeId")]
    public string? ProductSizeId { get; set; }

    [JsonPropertyName("ProductColorId")]
    public string? ProductColorId { get; set; }

    [JsonPropertyName("ProductConfigurationId")]
    public string? ProductConfigurationId { get; set; }

    [JsonPropertyName("ProductVersionId")]
    public string? ProductVersionId { get; set; }

    // Status and control
    [JsonPropertyName("ProductionOrderStatus")]
    public string? ProductionOrderStatus { get; set; }

    [JsonPropertyName("ProductionOrderRemainderStatus")]
    public string? ProductionOrderRemainderStatus { get; set; }

    [JsonPropertyName("IsProductionOrderSchedulingLocked")]
    public string? IsProductionOrderSchedulingLocked { get; set; }

    [JsonPropertyName("AreRouteJobsGenerated")]
    public string? AreRouteJobsGenerated { get; set; }

    [JsonPropertyName("IsProductionRouteOperationValid")]
    public string? IsProductionRouteOperationValid { get; set; }

    // Quantities
    [JsonPropertyName("ScheduledQuantity")]
    public decimal? ScheduledQuantity { get; set; }

    [JsonPropertyName("EstimatedQuantity")]
    public decimal? EstimatedQuantity { get; set; }

    [JsonPropertyName("StartedQuantity")]
    public decimal? StartedQuantity { get; set; }

    [JsonPropertyName("RemainingReportAsFinishedQuantity")]
    public decimal? RemainingReportAsFinishedQuantity { get; set; }

    [JsonPropertyName("ProductionOrderQuantity")]
    public decimal? ProductionOrderQuantity { get; set; }

    // Dates and times
    [JsonPropertyName("ScheduledDate")]
    public string? ScheduledDate { get; set; }

    [JsonPropertyName("ScheduledStartDate")]
    public string? ScheduledStartDate { get; set; }

    [JsonPropertyName("ScheduledStartTime")]
    public int? ScheduledStartTime { get; set; }

    [JsonPropertyName("ScheduledEndDate")]
    public string? ScheduledEndDate { get; set; }

    [JsonPropertyName("ScheduledEndTime")]
    public int? ScheduledEndTime { get; set; }

    [JsonPropertyName("EstimatedDate")]
    public string? EstimatedDate { get; set; }

    [JsonPropertyName("StartedDate")]
    public string? StartedDate { get; set; }

    [JsonPropertyName("EndedDate")]
    public string? EndedDate { get; set; }

    [JsonPropertyName("ReportedAsFinishedDate")]
    public string? ReportedAsFinishedDate { get; set; }

    [JsonPropertyName("ReleasedDate")]
    public string? ReleasedDate { get; set; }

    [JsonPropertyName("DeliveryDate")]
    public string? DeliveryDate { get; set; }

    [JsonPropertyName("DeliveryTime")]
    public int? DeliveryTime { get; set; }

    [JsonPropertyName("LastSchedulingDate")]
    public string? LastSchedulingDate { get; set; }

    [JsonPropertyName("LastSchedulingTime")]
    public int? LastSchedulingTime { get; set; }

    [JsonPropertyName("LastSchedulingDateDirection")]
    public string? LastSchedulingDateDirection { get; set; }

    // Location
    [JsonPropertyName("ProductionSiteId")]
    public string? ProductionSiteId { get; set; }

    [JsonPropertyName("ProductionWarehouseId")]
    public string? ProductionWarehouseId { get; set; }

    [JsonPropertyName("ProductionWarehouseLocationId")]
    public string? ProductionWarehouseLocationId { get; set; }

    // Groups and categories
    [JsonPropertyName("ProductionGroupId")]
    public string? ProductionGroupId { get; set; }

    [JsonPropertyName("ProductionPoolId")]
    public string? ProductionPoolId { get; set; }

    [JsonPropertyName("ProductionOrderName")]
    public string? ProductionOrderName { get; set; }

    // Source information
    [JsonPropertyName("SourcePlannedOrderNumber")]
    public string? SourcePlannedOrderNumber { get; set; }

    [JsonPropertyName("SourceMasterPlanId")]
    public string? SourceMasterPlanId { get; set; }

    [JsonPropertyName("SourceBOMId")]
    public string? SourceBOMId { get; set; }

    [JsonPropertyName("SourceRouteId")]
    public string? SourceRouteId { get; set; }

    [JsonPropertyName("ParentProductionOrderNumber")]
    public string? ParentProductionOrderNumber { get; set; }

    // Demand references
    [JsonPropertyName("DemandProductionOrderNumber")]
    public string? DemandProductionOrderNumber { get; set; }

    [JsonPropertyName("DemandSalesOrderNumber")]
    public string? DemandSalesOrderNumber { get; set; }

    // Inventory tracking
    [JsonPropertyName("InventoryLotId")]
    public string? InventoryLotId { get; set; }

    [JsonPropertyName("ItemBatchNumber")]
    public string? ItemBatchNumber { get; set; }

    [JsonPropertyName("ProductSerialNumber")]
    public string? ProductSerialNumber { get; set; }

    [JsonPropertyName("InventoryStatusId")]
    public string? InventoryStatusId { get; set; }

    // Policies and rules
    [JsonPropertyName("WarehouseReleaseReservationRequirementRule")]
    public string? WarehouseReleaseReservationRequirementRule { get; set; }

    [JsonPropertyName("AutoReservationMode")]
    public string? AutoReservationMode { get; set; }

    [JsonPropertyName("ProductionOrderLedgerPostingRule")]
    public string? ProductionOrderLedgerPostingRule { get; set; }

    [JsonPropertyName("SchedulingMethod")]
    public string? SchedulingMethod { get; set; }

    // Planning and priority
    [JsonPropertyName("PlanningPriority")]
    public decimal? PlanningPriority { get; set; }

    [JsonPropertyName("ProductionOrderPriority")]
    public int? ProductionOrderPriority { get; set; }

    [JsonPropertyName("ProductionLevel")]
    public int? ProductionLevel { get; set; }

    // Scheduling
    [JsonPropertyName("WorkingTimeSchedulingPropertyId")]
    public string? WorkingTimeSchedulingPropertyId { get; set; }

    // Financial dimensions
    [JsonPropertyName("DefaultLedgerDimensionDisplayValue")]
    public string? DefaultLedgerDimensionDisplayValue { get; set; }
}

/// <summary>
/// BOM Line
/// </summary>
public class BomLine
{
    [JsonPropertyName("dataAreaId")]
    public string? DataAreaId { get; set; }

    [JsonPropertyName("ProductionOrderNumber")]
    public string? ProductionOrderNumber { get; set; }

    [JsonPropertyName("LineNumber")]
    public decimal LineNumber { get; set; }

    [JsonPropertyName("ItemNumber")]
    public string? ItemNumber { get; set; }

    [JsonPropertyName("EstimatedBOMLineQuantity")]
    public decimal EstimatedBOMLineQuantity { get; set; }

    [JsonPropertyName("RemainingBOMLineQuantity")]
    public decimal RemainingBOMLineQuantity { get; set; }

    [JsonPropertyName("BOMLineUnitSymbol")]
    public string? BOMLineUnitSymbol { get; set; }

    [JsonPropertyName("ConsumptionSiteId")]
    public string? ConsumptionSiteId { get; set; }

    [JsonPropertyName("ConsumptionWarehouseId")]
    public string? ConsumptionWarehouseId { get; set; }

    [JsonPropertyName("FlushingPrinciple")]
    public string? FlushingPrinciple { get; set; }
}
