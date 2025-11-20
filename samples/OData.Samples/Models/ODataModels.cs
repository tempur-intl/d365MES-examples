using System.Text.Json.Serialization;

namespace OData.Samples.Models;

/// <summary>
/// Sample query configuration loaded from sample-queries.json
/// </summary>
public class SampleQueryConfig
{
    [JsonPropertyName("productionOrders")]
    public ProductionOrderQuery ProductionOrders { get; set; } = new();

    [JsonPropertyName("product")]
    public ProductQuery Product { get; set; } = new();

    [JsonPropertyName("bom")]
    public BomQuery Bom { get; set; } = new();

    [JsonPropertyName("route")]
    public RouteQuery Route { get; set; } = new();
}

public class ProductionOrderQuery
{
    [JsonPropertyName("filter")]
    public string Filter { get; set; } = string.Empty;

    [JsonPropertyName("top")]
    public int Top { get; set; }
}

public class ProductQuery
{
    [JsonPropertyName("productNumber")]
    public string ProductNumber { get; set; } = string.Empty;
}

public class BomQuery
{
    [JsonPropertyName("productionOrderNumber")]
    public string ProductionOrderNumber { get; set; } = string.Empty;
}

public class RouteQuery
{
    [JsonPropertyName("productionOrderNumber")]
    public string ProductionOrderNumber { get; set; } = string.Empty;
}

/// <summary>
/// OData response wrapper
/// </summary>
public class ODataResponse<T>
{
    [JsonPropertyName("@odata.context")]
    public string? ODataContext { get; set; }

    [JsonPropertyName("value")]
    public List<T> Value { get; set; } = new();
}

/// <summary>
/// Production order header entity - complete model matching D365
/// </summary>
public class ProductionOrder
{
    // Key fields
    [JsonPropertyName("dataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;

    [JsonPropertyName("ProductionOrderNumber")]
    public string ProductionOrderNumber { get; set; } = string.Empty;

    // Product identification
    [JsonPropertyName("ItemNumber")]
    public string ItemNumber { get; set; } = string.Empty;

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
    public string ProductionOrderStatus { get; set; } = string.Empty;

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
    public decimal ScheduledQuantity { get; set; }

    [JsonPropertyName("EstimatedQuantity")]
    public decimal EstimatedQuantity { get; set; }

    [JsonPropertyName("StartedQuantity")]
    public decimal StartedQuantity { get; set; }

    [JsonPropertyName("RemainingReportAsFinishedQuantity")]
    public decimal RemainingReportAsFinishedQuantity { get; set; }

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

    [JsonPropertyName("SourceBOMVersionValidityDate")]
    public string? SourceBOMVersionValidityDate { get; set; }

    [JsonPropertyName("ParentProductionOrderNumber")]
    public string? ParentProductionOrderNumber { get; set; }

    // Demand references
    [JsonPropertyName("DemandProductionOrderNumber")]
    public string? DemandProductionOrderNumber { get; set; }

    [JsonPropertyName("DemandProductionOrderLineNumber")]
    public string? DemandProductionOrderLineNumber { get; set; }

    [JsonPropertyName("DemandProductionOrderHeaderInventoryLotId")]
    public string? DemandProductionOrderHeaderInventoryLotId { get; set; }

    [JsonPropertyName("DemandProductionOrderLineInventoryLotId")]
    public string? DemandProductionOrderLineInventoryLotId { get; set; }

    [JsonPropertyName("DemandSalesOrderNumber")]
    public string? DemandSalesOrderNumber { get; set; }

    [JsonPropertyName("DemandSalesOrderLineInventoryLotId")]
    public string? DemandSalesOrderLineInventoryLotId { get; set; }

    [JsonPropertyName("DemandTransferOrderNumber")]
    public string? DemandTransferOrderNumber { get; set; }

    [JsonPropertyName("DemandTransferOrderLineReceivingInventoryLotId")]
    public string? DemandTransferOrderLineReceivingInventoryLotId { get; set; }

    [JsonPropertyName("DemandInventoryJournalNumber")]
    public string? DemandInventoryJournalNumber { get; set; }

    [JsonPropertyName("DemandInventoryJournalLineInventoryLotId")]
    public string? DemandInventoryJournalLineInventoryLotId { get; set; }

    // Inventory tracking
    [JsonPropertyName("InventoryLotId")]
    public string? InventoryLotId { get; set; }

    [JsonPropertyName("ItemBatchNumber")]
    public string? ItemBatchNumber { get; set; }

    [JsonPropertyName("ProductSerialNumber")]
    public string? ProductSerialNumber { get; set; }

    [JsonPropertyName("LicensePlateNumber")]
    public string? LicensePlateNumber { get; set; }

    [JsonPropertyName("InventoryStatusId")]
    public string? InventoryStatusId { get; set; }

    [JsonPropertyName("InventoryOwnerId")]
    public string? InventoryOwnerId { get; set; }

    // Policies and rules
    [JsonPropertyName("WarehouseReleaseReservationRequirementRule")]
    public string? WarehouseReleaseReservationRequirementRule { get; set; }

    [JsonPropertyName("AutoReservationMode")]
    public string? AutoReservationMode { get; set; }

    [JsonPropertyName("ProductionOrderLedgerPostingRule")]
    public string? ProductionOrderLedgerPostingRule { get; set; }

    [JsonPropertyName("ProductionOrderProfitSettingMethod")]
    public string? ProductionOrderProfitSettingMethod { get; set; }

    [JsonPropertyName("SchedulingMethod")]
    public string? SchedulingMethod { get; set; }

    // Conversion factors
    [JsonPropertyName("ProductionConsumptionWidthConversionFactor")]
    public decimal? ProductionConsumptionWidthConversionFactor { get; set; }

    [JsonPropertyName("ProductionConsumptionDepthConversionFactor")]
    public decimal? ProductionConsumptionDepthConversionFactor { get; set; }

    [JsonPropertyName("ProductionConsumptionDensityConversionFactor")]
    public decimal? ProductionConsumptionDensityConversionFactor { get; set; }

    [JsonPropertyName("ProductionConsumptionHeightConversionFactor")]
    public decimal? ProductionConsumptionHeightConversionFactor { get; set; }

    // Planning and priority
    [JsonPropertyName("PlanningPriority")]
    public decimal? PlanningPriority { get; set; }

    [JsonPropertyName("ProductionOrderPriority")]
    public int? ProductionOrderPriority { get; set; }

    [JsonPropertyName("ProductionLevel")]
    public int? ProductionLevel { get; set; }

    [JsonPropertyName("GanttChartColorNumber")]
    public int? GanttChartColorNumber { get; set; }

    // Subcontracting
    [JsonPropertyName("SubcontractingPurchaseOrderNumber")]
    public string? SubcontractingPurchaseOrderNumber { get; set; }

    [JsonPropertyName("SubcontractingPurchaseOrderLineInventoryLotId")]
    public string? SubcontractingPurchaseOrderLineInventoryLotId { get; set; }

    // Skip flags
    [JsonPropertyName("SkipCreateProductionRouteOperations")]
    public string? SkipCreateProductionRouteOperations { get; set; }

    [JsonPropertyName("SkipCreateProductionBOMLines")]
    public string? SkipCreateProductionBOMLines { get; set; }

    // Scheduling
    [JsonPropertyName("WorkingTimeSchedulingPropertyId")]
    public string? WorkingTimeSchedulingPropertyId { get; set; }

    // Financial dimensions
    [JsonPropertyName("DefaultLedgerDimensionDisplayValue")]
    public string? DefaultLedgerDimensionDisplayValue { get; set; }

    [JsonPropertyName("FinTagDisplayValue")]
    public string? FinTagDisplayValue { get; set; }

    // Production order quantity (added for compatibility)
    [JsonPropertyName("ProductionOrderQuantity")]
    public decimal ProductionOrderQuantity { get; set; }
}

/// <summary>
/// Production order BOM line entity - complete model matching D365
/// </summary>
public class BomLine
{
    // Key fields
    [JsonPropertyName("dataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;

    [JsonPropertyName("ProductionOrderNumber")]
    public string ProductionOrderNumber { get; set; } = string.Empty;

    [JsonPropertyName("LineNumber")]
    public decimal LineNumber { get; set; }

    // Item identification
    [JsonPropertyName("ItemNumber")]
    public string ItemNumber { get; set; } = string.Empty;

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

    // Line details
    [JsonPropertyName("LineType")]
    public string LineType { get; set; } = string.Empty;

    [JsonPropertyName("BOMLineQuantity")]
    public decimal BOMLineQuantity { get; set; }

    [JsonPropertyName("BOMLineQuantityDenominator")]
    public decimal BOMLineQuantityDenominator { get; set; }

    [JsonPropertyName("BOMLineUnitSymbol")]
    public string? BOMLineUnitSymbol { get; set; }

    // Quantities
    [JsonPropertyName("EstimatedBOMLineQuantity")]
    public decimal EstimatedBOMLineQuantity { get; set; }

    [JsonPropertyName("EstimatedInventoryQuantity")]
    public decimal EstimatedInventoryQuantity { get; set; }

    [JsonPropertyName("RemainingBOMLineQuantity")]
    public decimal RemainingBOMLineQuantity { get; set; }

    [JsonPropertyName("RemainingInventoryQuantity")]
    public decimal RemainingInventoryQuantity { get; set; }

    [JsonPropertyName("StartedBOMLineQuantity")]
    public decimal StartedBOMLineQuantity { get; set; }

    [JsonPropertyName("StartedInventoryQuantity")]
    public decimal StartedInventoryQuantity { get; set; }

    [JsonPropertyName("ReleasedBOMLineQuantity")]
    public decimal ReleasedBOMLineQuantity { get; set; }

    // Scrap
    [JsonPropertyName("VariableScrapPercentage")]
    public decimal VariableScrapPercentage { get; set; }

    [JsonPropertyName("ConstantScrapBOMLineQuantity")]
    public decimal ConstantScrapBOMLineQuantity { get; set; }

    // Location
    [JsonPropertyName("ConsumptionSiteId")]
    public string? ConsumptionSiteId { get; set; }

    [JsonPropertyName("ConsumptionWarehouseId")]
    public string? ConsumptionWarehouseId { get; set; }

    [JsonPropertyName("ConsumptionWarehouseLocationId")]
    public string? ConsumptionWarehouseLocationId { get; set; }

    // Route operation
    [JsonPropertyName("RouteOperationNumber")]
    public int? RouteOperationNumber { get; set; }

    [JsonPropertyName("SubRouteOperationId")]
    public string? SubRouteOperationId { get; set; }

    // Consumption
    [JsonPropertyName("FlushingPrinciple")]
    public string? FlushingPrinciple { get; set; }

    [JsonPropertyName("ConsumptionType")]
    public string? ConsumptionType { get; set; }

    [JsonPropertyName("ConsumptionCalculationMethod")]
    public string? ConsumptionCalculationMethod { get; set; }

    [JsonPropertyName("ConsumptionCalculationConstant")]
    public decimal? ConsumptionCalculationConstant { get; set; }

    // Status flags
    [JsonPropertyName("IsFullyConsumed")]
    public string? IsFullyConsumed { get; set; }

    [JsonPropertyName("IsConsumedAtOperationComplete")]
    public string? IsConsumedAtOperationComplete { get; set; }

    [JsonPropertyName("IsConstantConsumptionReleased")]
    public string? IsConstantConsumptionReleased { get; set; }

    [JsonPropertyName("IsResourceConsumptionUsed")]
    public bool? IsResourceConsumptionUsed { get; set; }

    [JsonPropertyName("LineRemainderStatus")]
    public string? LineRemainderStatus { get; set; }

    // Warehouse policies
    [JsonPropertyName("WarehouseBomReleaseReservationRequirementRule")]
    public string? WarehouseBomReleaseReservationRequirementRule { get; set; }

    [JsonPropertyName("AutoReservationMode")]
    public string? AutoReservationMode { get; set; }

    // Inventory tracking
    [JsonPropertyName("InventoryLotId")]
    public string? InventoryLotId { get; set; }

    [JsonPropertyName("ItemBatchNumber")]
    public string? ItemBatchNumber { get; set; }

    [JsonPropertyName("InventoryStatusId")]
    public string? InventoryStatusId { get; set; }

    [JsonPropertyName("InventoryOwnerId")]
    public string? InventoryOwnerId { get; set; }

    // Physical dimensions
    [JsonPropertyName("PhysicalProductDensity")]
    public decimal PhysicalProductDensity { get; set; }

    [JsonPropertyName("PhysicalProductHeight")]
    public decimal PhysicalProductHeight { get; set; }

    [JsonPropertyName("PhysicalProductDepth")]
    public decimal PhysicalProductDepth { get; set; }

    [JsonPropertyName("PhysicalProductWidth")]
    public decimal PhysicalProductWidth { get; set; }

    // Rounding
    [JsonPropertyName("RoundingUpMethod")]
    public string? RoundingUpMethod { get; set; }

    [JsonPropertyName("RoundingUpMultiplesBOMLineQuantity")]
    public decimal RoundingUpMultiplesBOMLineQuantity { get; set; }

    // Material overpick
    [JsonPropertyName("MaterialOverpickPercentage")]
    public decimal MaterialOverpickPercentage { get; set; }

    // Scheduling
    [JsonPropertyName("RawMaterialScheduledConsumptionDate")]
    public string? RawMaterialScheduledConsumptionDate { get; set; }

    [JsonPropertyName("RawMaterialScheduledConsumptionTime")]
    public int? RawMaterialScheduledConsumptionTime { get; set; }

    // Costing
    [JsonPropertyName("WillCostCalculationIncludeLine")]
    public string? WillCostCalculationIncludeLine { get; set; }

    // Position
    [JsonPropertyName("PositionNumber")]
    public string? PositionNumber { get; set; }

    // Source information
    [JsonPropertyName("SourceBOMId")]
    public string? SourceBOMId { get; set; }

    [JsonPropertyName("SubBOMId")]
    public string? SubBOMId { get; set; }

    [JsonPropertyName("SourcePlannedOrderNumber")]
    public string? SourcePlannedOrderNumber { get; set; }

    [JsonPropertyName("SourceMasterPlanId")]
    public string? SourceMasterPlanId { get; set; }

    // Demand references
    [JsonPropertyName("DemandProductionOrderNumber")]
    public string? DemandProductionOrderNumber { get; set; }

    [JsonPropertyName("DemandProductionOrderLineNumber")]
    public string? DemandProductionOrderLineNumber { get; set; }

    [JsonPropertyName("DemandProductionOrderHeaderInventoryLotId")]
    public string? DemandProductionOrderHeaderInventoryLotId { get; set; }

    [JsonPropertyName("DemandProductionOrderLineInventoryLotId")]
    public string? DemandProductionOrderLineInventoryLotId { get; set; }

    [JsonPropertyName("DemandSalesOrderNumber")]
    public string? DemandSalesOrderNumber { get; set; }

    [JsonPropertyName("DemandSalesOrderLineInventoryLotId")]
    public string? DemandSalesOrderLineInventoryLotId { get; set; }

    [JsonPropertyName("DemandTransferOrderNumber")]
    public string? DemandTransferOrderNumber { get; set; }

    [JsonPropertyName("DemandTransferOrderLineReceivingInventoryLotId")]
    public string? DemandTransferOrderLineReceivingInventoryLotId { get; set; }

    [JsonPropertyName("DemandInventoryJournalNumber")]
    public string? DemandInventoryJournalNumber { get; set; }

    [JsonPropertyName("DemandInventoryJournalLineInventoryLotId")]
    public string? DemandInventoryJournalLineInventoryLotId { get; set; }

    // Subcontracting
    [JsonPropertyName("SubcontractingPurchaseOrderNumber")]
    public string? SubcontractingPurchaseOrderNumber { get; set; }

    [JsonPropertyName("SubcontractingPurchaseOrderLineInventoryLotId")]
    public string? SubcontractingPurchaseOrderLineInventoryLotId { get; set; }

    [JsonPropertyName("SubcontractingVendorAccountNumber")]
    public string? SubcontractingVendorAccountNumber { get; set; }

    // Financial dimensions
    [JsonPropertyName("DefaultDimensionDisplayValue")]
    public string? DefaultDimensionDisplayValue { get; set; }

    // Legacy/compatibility properties
    [JsonPropertyName("BOMId")]
    public string? BOMId { get; set; }

    [JsonPropertyName("BOMQuantity")]
    public decimal? BOMQuantity { get; set; }

    [JsonPropertyName("BOMUnitSymbol")]
    public string? BOMUnitSymbol { get; set; }
}

/// <summary>
/// Production order route operation entity - complete model matching D365
/// </summary>
public class RouteOperation
{
    // Key fields
    [JsonPropertyName("dataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;

    [JsonPropertyName("ProductionOrderNumber")]
    public string ProductionOrderNumber { get; set; } = string.Empty;

    [JsonPropertyName("OperationNumber")]
    public int OperationNumber { get; set; }

    // Operation details
    [JsonPropertyName("OperationId")]
    public string OperationId { get; set; } = string.Empty;

    [JsonPropertyName("OperationPriority")]
    public string? OperationPriority { get; set; }

    [JsonPropertyName("RouteOperationSequence")]
    public int? RouteOperationSequence { get; set; }

    [JsonPropertyName("RouteType")]
    public string? RouteType { get; set; }

    // Timing
    [JsonPropertyName("ProcessTime")]
    public decimal? ProcessTime { get; set; }

    [JsonPropertyName("SetupTime")]
    public decimal? SetupTime { get; set; }

    [JsonPropertyName("QueueTimeBefore")]
    public decimal? QueueTimeBefore { get; set; }

    [JsonPropertyName("QueueTimeAfter")]
    public decimal? QueueTimeAfter { get; set; }

    [JsonPropertyName("TransitTime")]
    public decimal? TransitTime { get; set; }

    [JsonPropertyName("EstimatedProcessTime")]
    public decimal? EstimatedProcessTime { get; set; }

    [JsonPropertyName("EstimatedSetupTime")]
    public decimal? EstimatedSetupTime { get; set; }

    // Scheduling
    [JsonPropertyName("ScheduledFromDate")]
    public string? ScheduledFromDate { get; set; }

    [JsonPropertyName("ScheduledFromTime")]
    public int? ScheduledFromTime { get; set; }

    [JsonPropertyName("ScheduledEndDate")]
    public string? ScheduledEndDate { get; set; }

    [JsonPropertyName("ScheduledEndTime")]
    public int? ScheduledEndTime { get; set; }

    [JsonPropertyName("WorkingTimeSchedulingPropertyId")]
    public string? WorkingTimeSchedulingPropertyId { get; set; }

    // Quantities
    [JsonPropertyName("ProcessQuantity")]
    public decimal? ProcessQuantity { get; set; }

    [JsonPropertyName("EstimatedOperationQuantity")]
    public decimal? EstimatedOperationQuantity { get; set; }

    [JsonPropertyName("OverlapOperationQuantity")]
    public decimal? OverlapOperationQuantity { get; set; }

    [JsonPropertyName("TransferBatchQuantity")]
    public decimal? TransferBatchQuantity { get; set; }

    // Resources
    [JsonPropertyName("ResourceQuantity")]
    public int? ResourceQuantity { get; set; }

    [JsonPropertyName("CostingOperationResourceId")]
    public string? CostingOperationResourceId { get; set; }

    [JsonPropertyName("LoadPercentage")]
    public decimal? LoadPercentage { get; set; }

    // Route group
    [JsonPropertyName("RouteGroupId")]
    public string? RouteGroupId { get; set; }

    // Cost categories
    [JsonPropertyName("SetupCostCategoryId")]
    public string? SetupCostCategoryId { get; set; }

    [JsonPropertyName("ProcessCostCategoryId")]
    public string? ProcessCostCategoryId { get; set; }

    [JsonPropertyName("QuantityCostCategoryId")]
    public string? QuantityCostCategoryId { get; set; }

    // Job IDs
    [JsonPropertyName("SetupProductionJobId")]
    public string? SetupProductionJobId { get; set; }

    [JsonPropertyName("ProcessProductionJobId")]
    public string? ProcessProductionJobId { get; set; }

    // Completion
    [JsonPropertyName("SetupCompletionPercentage")]
    public decimal? SetupCompletionPercentage { get; set; }

    [JsonPropertyName("ProcessCompletionPercentage")]
    public decimal? ProcessCompletionPercentage { get; set; }

    // Status flags
    [JsonPropertyName("IsOperationStarted")]
    public string? IsOperationStarted { get; set; }

    [JsonPropertyName("IsOperationCompleted")]
    public string? IsOperationCompleted { get; set; }

    [JsonPropertyName("IsConstantConsumptionReleased")]
    public string? IsConstantConsumptionReleased { get; set; }

    [JsonPropertyName("RouteOperationRemainderStatus")]
    public string? RouteOperationRemainderStatus { get; set; }

    // Scrap
    [JsonPropertyName("ScrapPercentage")]
    public decimal? ScrapPercentage { get; set; }

    [JsonPropertyName("AccumulatedScrapPercentage")]
    public decimal? AccumulatedScrapPercentage { get; set; }

    // Linking
    [JsonPropertyName("NextOperationNumber")]
    public int? NextOperationNumber { get; set; }

    [JsonPropertyName("NextOperationLinkType")]
    public string? NextOperationLinkType { get; set; }

    // Conversion and rate
    [JsonPropertyName("OperationsTimeToHourConversionFactor")]
    public decimal? OperationsTimeToHourConversionFactor { get; set; }

    [JsonPropertyName("RouteOperationRateMethod")]
    public string? RouteOperationRateMethod { get; set; }

    // Financial dimensions
    [JsonPropertyName("DefaultLedgerDimensionDisplayValue")]
    public string? DefaultLedgerDimensionDisplayValue { get; set; }

    // Legacy/compatibility properties
    [JsonPropertyName("RouteId")]
    public string? RouteId { get; set; }

    [JsonPropertyName("TransportTimeTo")]
    public decimal? TransportTimeTo { get; set; }
}

/// <summary>
/// Released product V2 entity - complete model matching D365
/// NOTE: This entity has 400+ properties. Including most commonly used ones.
/// </summary>
public class ReleasedProduct
{
    // Key fields
    [JsonPropertyName("dataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;

    [JsonPropertyName("ItemNumber")]
    public string ItemNumber { get; set; } = string.Empty;

    [JsonPropertyName("ProductNumber")]
    public string ProductNumber { get; set; } = string.Empty;

    // Basic information
    [JsonPropertyName("ProductName")]
    public string? ProductName { get; set; }

    [JsonPropertyName("ProductType")]
    public string? ProductType { get; set; }

    [JsonPropertyName("ProductSubType")]
    public string? ProductSubType { get; set; }

    [JsonPropertyName("SearchName")]
    public string? SearchName { get; set; }

    [JsonPropertyName("ProductSearchName")]
    public string? ProductSearchName { get; set; }

    // Units
    [JsonPropertyName("InventoryUnitSymbol")]
    public string? InventoryUnitSymbol { get; set; }

    [JsonPropertyName("BOMUnitSymbol")]
    public string? BOMUnitSymbol { get; set; }

    [JsonPropertyName("PurchaseUnitSymbol")]
    public string? PurchaseUnitSymbol { get; set; }

    [JsonPropertyName("SalesUnitSymbol")]
    public string? SalesUnitSymbol { get; set; }

    [JsonPropertyName("CatchWeightUnitSymbol")]
    public string? CatchWeightUnitSymbol { get; set; }

    // Dimensions
    [JsonPropertyName("ProductDimensionGroupName")]
    public string? ProductDimensionGroupName { get; set; }

    [JsonPropertyName("StorageDimensionGroupName")]
    public string? StorageDimensionGroupName { get; set; }

    [JsonPropertyName("TrackingDimensionGroupName")]
    public string? TrackingDimensionGroupName { get; set; }

    // Default product dimensions
    [JsonPropertyName("DefaultProductColorId")]
    public string? DefaultProductColorId { get; set; }

    [JsonPropertyName("DefaultProductConfigurationId")]
    public string? DefaultProductConfigurationId { get; set; }

    [JsonPropertyName("DefaultProductSizeId")]
    public string? DefaultProductSizeId { get; set; }

    [JsonPropertyName("DefaultProductStyleId")]
    public string? DefaultProductStyleId { get; set; }

    [JsonPropertyName("DefaultProductVersionId")]
    public string? DefaultProductVersionId { get; set; }

    // Physical properties
    [JsonPropertyName("ProductVolume")]
    public decimal? ProductVolume { get; set; }

    [JsonPropertyName("NetProductWeight")]
    public decimal? NetProductWeight { get; set; }

    [JsonPropertyName("TareProductWeight")]
    public decimal? TareProductWeight { get; set; }

    [JsonPropertyName("GrossProductHeight")]
    public decimal? GrossProductHeight { get; set; }

    [JsonPropertyName("GrossProductWidth")]
    public decimal? GrossProductWidth { get; set; }

    [JsonPropertyName("GrossDepth")]
    public decimal? GrossDepth { get; set; }

    // Pricing
    [JsonPropertyName("SalesPrice")]
    public decimal? SalesPrice { get; set; }

    [JsonPropertyName("PurchasePrice")]
    public decimal? PurchasePrice { get; set; }

    [JsonPropertyName("UnitCost")]
    public decimal? UnitCost { get; set; }

    [JsonPropertyName("SalesPriceQuantity")]
    public decimal? SalesPriceQuantity { get; set; }

    [JsonPropertyName("PurchasePriceQuantity")]
    public decimal? PurchasePriceQuantity { get; set; }

    [JsonPropertyName("UnitCostQuantity")]
    public decimal? UnitCostQuantity { get; set; }

    // Production
    [JsonPropertyName("ProductionType")]
    public string? ProductionType { get; set; }

    [JsonPropertyName("ProductionGroupId")]
    public string? ProductionGroupId { get; set; }

    [JsonPropertyName("ProductionPoolId")]
    public string? ProductionPoolId { get; set; }

    [JsonPropertyName("FlushingPrinciple")]
    public string? FlushingPrinciple { get; set; }

    [JsonPropertyName("BOMLevel")]
    public int? BOMLevel { get; set; }

    // Inventory
    [JsonPropertyName("ItemModelGroupId")]
    public string? ItemModelGroupId { get; set; }

    [JsonPropertyName("ProductCoverageGroupId")]
    public string? ProductCoverageGroupId { get; set; }

    // Lifecycle
    [JsonPropertyName("ProductLifecycleStateId")]
    public string? ProductLifecycleStateId { get; set; }

    [JsonPropertyName("ProductLifeCycleValidFromDate")]
    public string? ProductLifeCycleValidFromDate { get; set; }

    [JsonPropertyName("ProductLifeCycleValidToDate")]
    public string? ProductLifeCycleValidToDate { get; set; }

    // Warehouse
    [JsonPropertyName("WarehouseMobileDeviceDescriptionLine1")]
    public string? WarehouseMobileDeviceDescriptionLine1 { get; set; }

    [JsonPropertyName("WarehouseMobileDeviceDescriptionLine2")]
    public string? WarehouseMobileDeviceDescriptionLine2 { get; set; }

    // Batch/Serial
    [JsonPropertyName("IsBatchNumberActive")]
    public string? IsBatchNumberActive { get; set; }

    [JsonPropertyName("IsSerialNumberActive")]
    public string? IsSerialNumberActive { get; set; }

    [JsonPropertyName("BatchNumberGroupCode")]
    public string? BatchNumberGroupCode { get; set; }

    [JsonPropertyName("SerialNumberGroupCode")]
    public string? SerialNumberGroupCode { get; set; }

    // Catch weight
    [JsonPropertyName("IsCatchWeightProduct")]
    public string? IsCatchWeightProduct { get; set; }

    [JsonPropertyName("MinimumCatchWeightQuantity")]
    public decimal? MinimumCatchWeightQuantity { get; set; }

    [JsonPropertyName("MaximumCatchWeightQuantity")]
    public decimal? MaximumCatchWeightQuantity { get; set; }

    // Tax
    [JsonPropertyName("SalesSalesTaxItemGroupCode")]
    public string? SalesSalesTaxItemGroupCode { get; set; }

    [JsonPropertyName("PurchaseSalesTaxItemGroupCode")]
    public string? PurchaseSalesTaxItemGroupCode { get; set; }

    // Vendor
    [JsonPropertyName("PrimaryVendorAccountNumber")]
    public string? PrimaryVendorAccountNumber { get; set; }

    // Groups
    [JsonPropertyName("ProductGroupId")]
    public string? ProductGroupId { get; set; }

    [JsonPropertyName("CostGroupId")]
    public string? CostGroupId { get; set; }

    [JsonPropertyName("BuyerGroupId")]
    public string? BuyerGroupId { get; set; }

    // Financial dimensions
    [JsonPropertyName("DefaultLedgerDimensionDisplayValue")]
    public string? DefaultLedgerDimensionDisplayValue { get; set; }
}


