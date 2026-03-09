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

    [JsonPropertyName("reportAsFinished")]
    public ReportAsFinishedData ReportAsFinished { get; set; } = new();

    [JsonPropertyName("movementWork")]
    public MovementWorkData MovementWork { get; set; } = new();

    [JsonPropertyName("inventCountJournal")]
    public InventCountJournalData InventCountJournal { get; set; } = new();

    [JsonPropertyName("updateBatchDisposition")]
    public UpdateBatchDispositionData UpdateBatchDisposition { get; set; } = new();
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

public class MovementWorkData
{
    [JsonPropertyName("licensePlate")]
    public string LicensePlate { get; set; } = string.Empty;

    [JsonPropertyName("sourceLocation")]
    public string SourceLocation { get; set; } = string.Empty;

    [JsonPropertyName("destinationLocation")]
    public string DestinationLocation { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; } = 0;

    [JsonPropertyName("itemId")]
    public string ItemId { get; set; } = string.Empty;
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

    /// <summary>Valid values: <c>FlushingPrincip</c> | <c>Always</c> | <c>Never</c></summary>
    [JsonPropertyName("AutomaticBOMConsumptionRule")]
    public string? AutomaticBOMConsumptionRule { get; set; }

    /// <summary>Valid values: <c>RouteDependent</c> | <c>Always</c> | <c>Never</c></summary>
    [JsonPropertyName("AutomaticRouteConsumptionRule")]
    public string? AutomaticRouteConsumptionRule { get; set; }
}
public class ReportAsFinishedMessage
{
    [JsonPropertyName("ProductionOrderNumber")]
    public required string ProductionOrderNumber { get; set; }

    [JsonPropertyName("ReportFinishedLines")]
    public List<ReportFinishedLine> ReportFinishedLines { get; set; } = new();

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
    [JsonPropertyName("PrintLabel")]
    public string? PrintLabel { get; set; }
}

public class ReportFinishedLine
{
    [JsonPropertyName("LineNumber")]
    public decimal? LineNumber { get; set; }

    [JsonPropertyName("ItemNumber")]
    public string? ItemNumber { get; set; }

    /// <summary>Valid values: <c>MainItem</c> | <c>Formula</c> | <c>BOM</c> | <c>Co_Product</c> | <c>By_Product</c> | <c>None</c> (extensible)</summary>
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

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
    [JsonPropertyName("AcceptError")]
    public string? AcceptError { get; set; }

    /// <summary>Valid values: <c>None</c> | <c>Material</c> | <c>Machine</c> | <c>OperatingStaff</c> (extensible)</summary>
    [JsonPropertyName("ErrorCause")]
    public string? ErrorCause { get; set; }

    [JsonPropertyName("ExecutedDateTime")]
    public string? ExecutedDateTime { get; set; }

    [JsonPropertyName("ReportAsFinishedDate")]
    public string? ReportAsFinishedDate { get; set; }

    /// <summary>Valid values: <c>FlushingPrincip</c> | <c>Always</c> | <c>Never</c></summary>
    [JsonPropertyName("AutomaticBOMConsumptionRule")]
    public string? AutomaticBOMConsumptionRule { get; set; }

    /// <summary>Valid values: <c>RouteDependent</c> | <c>Always</c> | <c>Never</c></summary>
    [JsonPropertyName("AutomaticRouteConsumptionRule")]
    public string? AutomaticRouteConsumptionRule { get; set; }

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
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

    /// <summary>Valid values: <c>Yes</c> | <c>No</c>. Marks this as the final job for the order.</summary>
    [JsonPropertyName("EndJob")]
    public string? EndJob { get; set; }

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
    [JsonPropertyName("EndPickingList")]
    public string? EndPickingList { get; set; }

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
    [JsonPropertyName("EndRouteCard")]
    public string? EndRouteCard { get; set; }

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
    [JsonPropertyName("PostNow")]
    public string? PostNow { get; set; }

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
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

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
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

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
    [JsonPropertyName("IsConsumptionEnded")]
    public string? IsConsumptionEnded { get; set; }

    /// <summary>Valid values: <c>None</c> | <c>Material</c> | <c>Machine</c> | <c>OperatingStaff</c> (extensible)</summary>
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

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
    [JsonPropertyName("UseTimeAndAttendanceCost")]
    public string? UseTimeAndAttendanceCost { get; set; }

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
    [JsonPropertyName("AutoReportAsFinished")]
    public string? AutoReportAsFinished { get; set; }

    /// <summary>Valid values: <c>Yes</c> | <c>No</c></summary>
    [JsonPropertyName("AutoUpdate")]
    public string? AutoUpdate { get; set; }
}

/// <summary>
/// Contract for the TSI MES movement work service.
/// Only <see cref="LicensePlate"/> is required; all other fields default to empty/zero.
/// </summary>
public class MovementWorkContract
{
    [JsonPropertyName("LicensePlate")]
    public required string LicensePlate { get; set; }

    [JsonPropertyName("DataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;

    [JsonPropertyName("SourceLocation")]
    public string SourceLocation { get; set; } = string.Empty;

    [JsonPropertyName("DestinationLocation")]
    public string DestinationLocation { get; set; } = string.Empty;

    [JsonPropertyName("Quantity")]
    public decimal Quantity { get; set; } = 0;

    [JsonPropertyName("ItemId")]
    public string ItemId { get; set; } = string.Empty;
}

public class UpdateBatchDispositionData
{
    [JsonPropertyName("productionOrderNumber")]
    public string ProductionOrderNumber { get; set; } = string.Empty;

    [JsonPropertyName("itemNumber")]
    public string ItemNumber { get; set; } = string.Empty;

    [JsonPropertyName("batchNumber")]
    public string BatchNumber { get; set; } = string.Empty;

    [JsonPropertyName("dispositionCode")]
    public string DispositionCode { get; set; } = string.Empty;
}

public class InventCountJournalData
{
    [JsonPropertyName("productionOrderNumber")]
    public string ProductionOrderNumber { get; set; } = string.Empty;

    [JsonPropertyName("itemNumber")]
    public string ItemNumber { get; set; } = string.Empty;

    [JsonPropertyName("site")]
    public string Site { get; set; } = string.Empty;

    [JsonPropertyName("warehouse")]
    public string Warehouse { get; set; } = string.Empty;

    [JsonPropertyName("batchNumber")]
    public string BatchNumber { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("licensePlate")]
    public string LicensePlate { get; set; } = string.Empty;

    [JsonPropertyName("countedQuantity")]
    public decimal CountedQuantity { get; set; }

    [JsonPropertyName("countDate")]
    public string CountDate { get; set; } = string.Empty;
}

/// <summary>
/// Message content for the <c>TSIInventCountJournal</c> MES message type.
/// Creates an inventory counting journal line in D365 for a specific item/location/license plate.
/// </summary>
public class InventCountJournalMessage
{
    [JsonPropertyName("ProductionOrderNumber")]
    public required string ProductionOrderNumber { get; set; }

    [JsonPropertyName("ItemNumber")]
    public required string ItemNumber { get; set; }

    [JsonPropertyName("Site")]
    public string? Site { get; set; }

    [JsonPropertyName("Warehouse")]
    public string? Warehouse { get; set; }

    [JsonPropertyName("BatchNumber")]
    public string? BatchNumber { get; set; }

    [JsonPropertyName("Location")]
    public required string Location { get; set; }

    [JsonPropertyName("LicensePlate")]
    public string? LicensePlate { get; set; }

    [JsonPropertyName("CountedQuantity")]
    public required string CountedQuantity { get; set; }

    [JsonPropertyName("CountDate")]
    public required string CountDate { get; set; }
}

/// <summary>
/// Message content for the <c>TSIUpdateBatchDisposition</c> MES message type.
/// Updates the batch disposition code for a specific item/batch in D365.
/// </summary>
public class UpdateBatchDispositionMessage
{
    [JsonPropertyName("ProductionOrderNumber")]
    public required string ProductionOrderNumber { get; set; }

    [JsonPropertyName("ItemNumber")]
    public required string ItemNumber { get; set; }

    [JsonPropertyName("BatchNumber")]
    public required string BatchNumber { get; set; }

    [JsonPropertyName("DispositionCode")]
    public required string DispositionCode { get; set; }
}

/// <summary>
/// Request wrapper for <c>TSIMesWebServices/TSIMesWebService/process</c>.
/// </summary>
public class MovementWorkRequest
{
    [JsonPropertyName("_contract")]
    public required MovementWorkContract Contract { get; set; }
}
