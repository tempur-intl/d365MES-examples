using System.Text.Json.Serialization;

namespace MesIntegration.Samples.Models;

/// <summary>
/// Sample data configuration loaded from sample-data.json
/// </summary>
public class SampleDataConfig
{
    [JsonPropertyName("productionOrderNumber")]
    public string ProductionOrderNumber { get; set; } = string.Empty;

    [JsonPropertyName("startedQuantity")]
    public decimal StartedQuantity { get; set; }

    [JsonPropertyName("automaticBOMConsumptionRule")]
    public string AutomaticBOMConsumptionRule { get; set; } = string.Empty;

    [JsonPropertyName("automaticRouteConsumptionRule")]
    public string AutomaticRouteConsumptionRule { get; set; } = string.Empty;

    [JsonPropertyName("materialConsumption")]
    public List<MaterialConsumptionData> MaterialConsumption { get; set; } = new();

    [JsonPropertyName("timeConsumption")]
    public List<TimeConsumptionData> TimeConsumption { get; set; } = new();

    [JsonPropertyName("reportAsFinished")]
    public ReportAsFinishedData ReportAsFinished { get; set; } = new();
}

public class MaterialConsumptionData
{
    [JsonPropertyName("itemNumber")]
    public string ItemNumber { get; set; } = string.Empty;

    [JsonPropertyName("consumptionBOMQuantity")]
    public decimal ConsumptionBOMQuantity { get; set; }

    [JsonPropertyName("operationNumber")]
    public int OperationNumber { get; set; }

    [JsonPropertyName("productionSiteId")]
    public string ProductionSiteId { get; set; } = string.Empty;

    [JsonPropertyName("productionWarehouseId")]
    public string ProductionWarehouseId { get; set; } = string.Empty;
}

public class TimeConsumptionData
{
    [JsonPropertyName("operationNumber")]
    public int OperationNumber { get; set; }

    [JsonPropertyName("hours")]
    public decimal Hours { get; set; }

    [JsonPropertyName("goodQuantity")]
    public decimal GoodQuantity { get; set; }

    [JsonPropertyName("errorQuantity")]
    public decimal ErrorQuantity { get; set; }

    [JsonPropertyName("operationsResourceId")]
    public string OperationsResourceId { get; set; } = string.Empty;

    [JsonPropertyName("worker")]
    public string Worker { get; set; } = string.Empty;
}

public class ReportAsFinishedData
{
    [JsonPropertyName("itemNumber")]
    public string ItemNumber { get; set; } = string.Empty;

    [JsonPropertyName("reportedGoodQuantity")]
    public decimal ReportedGoodQuantity { get; set; }

    [JsonPropertyName("reportedErrorQuantity")]
    public decimal ReportedErrorQuantity { get; set; }

    [JsonPropertyName("productionSiteId")]
    public string ProductionSiteId { get; set; } = string.Empty;

    [JsonPropertyName("productionWarehouseId")]
    public string ProductionWarehouseId { get; set; } = string.Empty;

    [JsonPropertyName("productionWarehouseLocationId")]
    public string ProductionWarehouseLocationId { get; set; } = string.Empty;

    [JsonPropertyName("automaticBOMConsumptionRule")]
    public string AutomaticBOMConsumptionRule { get; set; } = string.Empty;

    [JsonPropertyName("automaticRouteConsumptionRule")]
    public string AutomaticRouteConsumptionRule { get; set; } = string.Empty;

    [JsonPropertyName("endJob")]
    public string EndJob { get; set; } = string.Empty;

    [JsonPropertyName("generateLicensePlate")]
    public string GenerateLicensePlate { get; set; } = string.Empty;

    [JsonPropertyName("printLabel")]
    public string PrintLabel { get; set; } = string.Empty;
}

/// <summary>
/// Base message envelope for MES integration
/// </summary>
public class MesMessageEnvelope
{
    [JsonPropertyName("_companyId")]
    public required string CompanyId { get; set; }

    [JsonPropertyName("_messageQueue")]
    public string MessageQueue { get; set; } = "JmgMES3P";

    [JsonPropertyName("_messageType")]
    public required string MessageType { get; set; }

    [JsonPropertyName("_messageContent")]
    public required string MessageContent { get; set; }
}

/// <summary>
/// Message to start a production order
/// </summary>
public class StartProductionOrderMessage
{
    [JsonPropertyName("ProductionOrderNumber")]
    public required string ProductionOrderNumber { get; set; }

    [JsonPropertyName("StartedQuantity")]
    public decimal? StartedQuantity { get; set; }

    [JsonPropertyName("StartedDate")]
    public string? StartedDate { get; set; }

    [JsonPropertyName("AutomaticBOMConsumptionRule")]
    public string? AutomaticBOMConsumptionRule { get; set; }

    [JsonPropertyName("AutomaticRouteConsumptionRule")]
    public string? AutomaticRouteConsumptionRule { get; set; }
}

/// <summary>
/// Message to report production as finished
/// </summary>
public class ReportAsFinishedMessage
{
    [JsonPropertyName("ProductionOrderNumber")]
    public required string ProductionOrderNumber { get; set; }

    [JsonPropertyName("ReportFinishedLines")]
    public List<ReportFinishedLine> ReportFinishedLines { get; set; } = new();

    [JsonPropertyName("PrintLabel")]
    public string? PrintLabel { get; set; }
}

public class ReportFinishedLine
{
    [JsonPropertyName("LineNumber")]
    public decimal? LineNumber { get; set; }

    [JsonPropertyName("ItemNumber")]
    public string? ItemNumber { get; set; }

    [JsonPropertyName("ProductionType")]
    public string? ProductionType { get; set; }

    [JsonPropertyName("ReportedErrorQuantity")]
    public decimal? ReportedErrorQuantity { get; set; }

    [JsonPropertyName("ReportedGoodQuantity")]
    public decimal? ReportedGoodQuantity { get; set; }

    [JsonPropertyName("ReportedErrorCatchWeightQuantity")]
    public decimal? ReportedErrorCatchWeightQuantity { get; set; }

    [JsonPropertyName("ReportedGoodCatchWeightQuantity")]
    public decimal? ReportedGoodCatchWeightQuantity { get; set; }

    [JsonPropertyName("AcceptError")]
    public string? AcceptError { get; set; }

    [JsonPropertyName("ErrorCause")]
    public string? ErrorCause { get; set; }

    [JsonPropertyName("ExecutedDateTime")]
    public string? ExecutedDateTime { get; set; }

    [JsonPropertyName("ReportAsFinishedDate")]
    public string? ReportAsFinishedDate { get; set; }

    [JsonPropertyName("AutomaticBOMConsumptionRule")]
    public string? AutomaticBOMConsumptionRule { get; set; }

    [JsonPropertyName("AutomaticRouteConsumptionRule")]
    public string? AutomaticRouteConsumptionRule { get; set; }

    [JsonPropertyName("RespectFlushingPrincipleDuringOverproduction")]
    public string? RespectFlushingPrincipleDuringOverproduction { get; set; }

    [JsonPropertyName("JournalNameId")]
    public string? JournalNameId { get; set; }

    [JsonPropertyName("PickingListJournalNameId")]
    public string? PickingListJournalNameId { get; set; }

    [JsonPropertyName("RouteCardJournalNameId")]
    public string? RouteCardJournalNameId { get; set; }

    [JsonPropertyName("FromOperationNumber")]
    public int? FromOperationNumber { get; set; }

    [JsonPropertyName("ToOperationNumber")]
    public int? ToOperationNumber { get; set; }

    [JsonPropertyName("InventoryLotId")]
    public string? InventoryLotId { get; set; }

    [JsonPropertyName("BaseValue")]
    public string? BaseValue { get; set; }

    [JsonPropertyName("EndJob")]
    public string? EndJob { get; set; }

    [JsonPropertyName("EndPickingList")]
    public string? EndPickingList { get; set; }

    [JsonPropertyName("EndRouteCard")]
    public string? EndRouteCard { get; set; }

    [JsonPropertyName("PostNow")]
    public string? PostNow { get; set; }

    [JsonPropertyName("AutoUpdate")]
    public string? AutoUpdate { get; set; }

    [JsonPropertyName("ProductColorId")]
    public string? ProductColorId { get; set; }

    [JsonPropertyName("ProductConfigurationId")]
    public string? ProductConfigurationId { get; set; }

    [JsonPropertyName("ProductSizeId")]
    public string? ProductSizeId { get; set; }

    [JsonPropertyName("ProductStyleId")]
    public string? ProductStyleId { get; set; }

    [JsonPropertyName("ProductVersionId")]
    public string? ProductVersionId { get; set; }

    [JsonPropertyName("ItemBatchNumber")]
    public string? ItemBatchNumber { get; set; }

    [JsonPropertyName("ProductSerialNumber")]
    public string? ProductSerialNumber { get; set; }

    [JsonPropertyName("GenerateLicensePlate")]
    public string? GenerateLicensePlate { get; set; }

    [JsonPropertyName("LicensePlateNumber")]
    public string? LicensePlateNumber { get; set; }

    [JsonPropertyName("InventoryStatusId")]
    public string? InventoryStatusId { get; set; }

    [JsonPropertyName("ProductionWarehouseId")]
    public string? ProductionWarehouseId { get; set; }

    [JsonPropertyName("ProductionSiteId")]
    public string? ProductionSiteId { get; set; }

    [JsonPropertyName("ProductionWarehouseLocationId")]
    public string? ProductionWarehouseLocationId { get; set; }

    [JsonPropertyName("InventoryDimension1")]
    public string? InventoryDimension1 { get; set; }

    [JsonPropertyName("InventoryDimension2")]
    public string? InventoryDimension2 { get; set; }

    [JsonPropertyName("InventoryDimension3")]
    public string? InventoryDimension3 { get; set; }

    [JsonPropertyName("InventoryDimension4")]
    public string? InventoryDimension4 { get; set; }

    [JsonPropertyName("InventoryDimension5")]
    public string? InventoryDimension5 { get; set; }

    [JsonPropertyName("InventoryDimension6")]
    public string? InventoryDimension6 { get; set; }

    [JsonPropertyName("InventoryDimension7")]
    public string? InventoryDimension7 { get; set; }

    [JsonPropertyName("InventoryDimension8")]
    public string? InventoryDimension8 { get; set; }

    [JsonPropertyName("InventoryDimension9")]
    public string? InventoryDimension9 { get; set; }

    [JsonPropertyName("InventoryDimension10")]
    public string? InventoryDimension10 { get; set; }

    [JsonPropertyName("InventoryDimension11")]
    public string? InventoryDimension11 { get; set; }

    [JsonPropertyName("InventoryDimension12")]
    public string? InventoryDimension12 { get; set; }
}

/// <summary>
/// Message to report material consumption (picking list)
/// </summary>
public class MaterialConsumptionMessage
{
    [JsonPropertyName("ProductionOrderNumber")]
    public required string ProductionOrderNumber { get; set; }

    [JsonPropertyName("PickingListLines")]
    public List<PickingListLine> PickingListLines { get; set; } = new();

    [JsonPropertyName("JournalNameId")]
    public string? JournalNameId { get; set; }
}

public class PickingListLine
{
    [JsonPropertyName("ItemNumber")]
    public required string ItemNumber { get; set; }

    [JsonPropertyName("ConsumptionBOMQuantity")]
    public decimal? ConsumptionBOMQuantity { get; set; }

    [JsonPropertyName("ProposalBOMQuantity")]
    public decimal? ProposalBOMQuantity { get; set; }

    [JsonPropertyName("ScrapBOMQuantity")]
    public decimal? ScrapBOMQuantity { get; set; }

    [JsonPropertyName("BOMUnitSymbol")]
    public string? BOMUnitSymbol { get; set; }

    [JsonPropertyName("ConsumptionInventoryQuantity")]
    public decimal? ConsumptionInventoryQuantity { get; set; }

    [JsonPropertyName("ProposalInventoryQuantity")]
    public decimal? ProposalInventoryQuantity { get; set; }

    [JsonPropertyName("ConsumptionCatchWeightQuantity")]
    public decimal? ConsumptionCatchWeightQuantity { get; set; }

    [JsonPropertyName("ProposalCatchWeightQuantity")]
    public decimal? ProposalCatchWeightQuantity { get; set; }

    [JsonPropertyName("ConsumptionDate")]
    public string? ConsumptionDate { get; set; }

    [JsonPropertyName("OperationNumber")]
    public int? OperationNumber { get; set; }

    [JsonPropertyName("LineNumber")]
    public decimal? LineNumber { get; set; }

    [JsonPropertyName("PositionNumber")]
    public string? PositionNumber { get; set; }

    [JsonPropertyName("IsConsumptionEnded")]
    public string? IsConsumptionEnded { get; set; }

    [JsonPropertyName("ErrorCause")]
    public string? ErrorCause { get; set; }

    [JsonPropertyName("InventoryLotId")]
    public string? InventoryLotId { get; set; }

    [JsonPropertyName("ProductColorId")]
    public string? ProductColorId { get; set; }

    [JsonPropertyName("ProductConfigurationId")]
    public string? ProductConfigurationId { get; set; }

    [JsonPropertyName("ProductSizeId")]
    public string? ProductSizeId { get; set; }

    [JsonPropertyName("ProductStyleId")]
    public string? ProductStyleId { get; set; }

    [JsonPropertyName("ProductVersionId")]
    public string? ProductVersionId { get; set; }

    [JsonPropertyName("ItemBatchNumber")]
    public string? ItemBatchNumber { get; set; }

    [JsonPropertyName("ProductSerialNumber")]
    public string? ProductSerialNumber { get; set; }

    [JsonPropertyName("LicensePlateNumber")]
    public string? LicensePlateNumber { get; set; }

    [JsonPropertyName("InventoryStatusId")]
    public string? InventoryStatusId { get; set; }

    [JsonPropertyName("ProductionWarehouseId")]
    public string? ProductionWarehouseId { get; set; }

    [JsonPropertyName("ProductionSiteId")]
    public string? ProductionSiteId { get; set; }

    [JsonPropertyName("ProductionWarehouseLocationId")]
    public string? ProductionWarehouseLocationId { get; set; }

    [JsonPropertyName("InventoryDimension1")]
    public string? InventoryDimension1 { get; set; }

    [JsonPropertyName("InventoryDimension2")]
    public string? InventoryDimension2 { get; set; }

    [JsonPropertyName("InventoryDimension3")]
    public string? InventoryDimension3 { get; set; }

    [JsonPropertyName("InventoryDimension4")]
    public string? InventoryDimension4 { get; set; }

    [JsonPropertyName("InventoryDimension5")]
    public string? InventoryDimension5 { get; set; }

    [JsonPropertyName("InventoryDimension6")]
    public string? InventoryDimension6 { get; set; }

    [JsonPropertyName("InventoryDimension7")]
    public string? InventoryDimension7 { get; set; }

    [JsonPropertyName("InventoryDimension8")]
    public string? InventoryDimension8 { get; set; }

    [JsonPropertyName("InventoryDimension9")]
    public string? InventoryDimension9 { get; set; }

    [JsonPropertyName("InventoryDimension10")]
    public string? InventoryDimension10 { get; set; }

    [JsonPropertyName("InventoryDimension11")]
    public string? InventoryDimension11 { get; set; }

    [JsonPropertyName("InventoryDimension12")]
    public string? InventoryDimension12 { get; set; }
}

/// <summary>
/// Message to report time consumed (route card)
/// </summary>
public class RouteCardMessage
{
    [JsonPropertyName("ProductionOrderNumber")]
    public required string ProductionOrderNumber { get; set; }

    [JsonPropertyName("RouteCardLines")]
    public List<RouteCardLine> RouteCardLines { get; set; } = new();

    [JsonPropertyName("JournalNameId")]
    public string? JournalNameId { get; set; }
}

public class RouteCardLine
{
    [JsonPropertyName("OperationNumber")]
    public required int OperationNumber { get; set; }

    [JsonPropertyName("OperationPriority")]
    public string? OperationPriority { get; set; }

    [JsonPropertyName("OperationId")]
    public string? OperationId { get; set; }

    [JsonPropertyName("OperationsResourceId")]
    public string? OperationsResourceId { get; set; }

    [JsonPropertyName("Worker")]
    public string? Worker { get; set; }

    [JsonPropertyName("HoursRouteCostCategoryId")]
    public string? HoursRouteCostCategoryId { get; set; }

    [JsonPropertyName("QuantityRouteCostCategoryId")]
    public string? QuantityRouteCostCategoryId { get; set; }

    [JsonPropertyName("HourlyRate")]
    public decimal? HourlyRate { get; set; }

    [JsonPropertyName("Hours")]
    public decimal? Hours { get; set; }

    [JsonPropertyName("GoodQuantity")]
    public decimal? GoodQuantity { get; set; }

    [JsonPropertyName("ErrorQuantity")]
    public decimal? ErrorQuantity { get; set; }

    [JsonPropertyName("CatchWeightGoodQuantity")]
    public decimal? CatchWeightGoodQuantity { get; set; }

    [JsonPropertyName("CatchWeightErrorQuantity")]
    public decimal? CatchWeightErrorQuantity { get; set; }

    [JsonPropertyName("QuantityPrice")]
    public decimal? QuantityPrice { get; set; }

    [JsonPropertyName("ProcessingPercentage")]
    public decimal? ProcessingPercentage { get; set; }

    [JsonPropertyName("ConsumptionDate")]
    public string? ConsumptionDate { get; set; }

    [JsonPropertyName("TaskType")]
    public string? TaskType { get; set; }

    [JsonPropertyName("ErrorCause")]
    public string? ErrorCause { get; set; }

    [JsonPropertyName("OperationCompleted")]
    public string? OperationCompleted { get; set; }

    [JsonPropertyName("BOMConsumption")]
    public string? BOMConsumption { get; set; }

    [JsonPropertyName("ReportAsFinished")]
    public string? ReportAsFinished { get; set; }
}

/// <summary>
/// Message to end a production order
/// </summary>
public class EndProductionOrderMessage
{
    [JsonPropertyName("ProductionOrderNumber")]
    public required string ProductionOrderNumber { get; set; }

    [JsonPropertyName("ExecutedDateTime")]
    public string? ExecutedDateTime { get; set; }

    [JsonPropertyName("EndedDate")]
    public string? EndedDate { get; set; }

    [JsonPropertyName("UseTimeAndAttendanceCost")]
    public string? UseTimeAndAttendanceCost { get; set; }

    [JsonPropertyName("AutoReportAsFinished")]
    public string? AutoReportAsFinished { get; set; }

    [JsonPropertyName("AutoUpdate")]
    public string? AutoUpdate { get; set; }
}
