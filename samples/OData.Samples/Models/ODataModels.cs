using System.Text.Json.Serialization;

namespace OData.Samples.Models;

/// <summary>
/// Sample query configuration loaded from sample-queries.json
/// </summary>
public class SampleQueryConfig
{
    [JsonPropertyName("tsiItems")]
    public TsiItemQuery TsiItems { get; set; } = new();

    [JsonPropertyName("tsiProdBomLines")]
    public TsiProdBomLinesQuery TsiProdBomLines { get; set; } = new();

    [JsonPropertyName("tsiLabels")]
    public TsiLabelQuery TsiLabels { get; set; } = new();

    [JsonPropertyName("tsiJobs")]
    public TsiJobQuery TsiJobs { get; set; } = new();

    [JsonPropertyName("warehouseWorkLines")]
    public WarehouseWorkLinesQuery WarehouseWorkLines { get; set; } = new();

    [JsonPropertyName("itemBatches")]
    public ItemBatchesQuery ItemBatches { get; set; } = new();
}

public class TsiItemQuery
{
    [JsonPropertyName("itemId")]
    public string ItemId { get; set; } = string.Empty;
}

public class TsiProdBomLinesQuery
{
    [JsonPropertyName("prodId")]
    public string ProdId { get; set; } = string.Empty;
}

public class TsiLabelQuery
{
    [JsonPropertyName("prodId")]
    public string ProdId { get; set; } = string.Empty;

    [JsonPropertyName("udiUnit")]
    public string UDIUnit { get; set; } = "x1";
}

public class TsiJobQuery
{
    [JsonPropertyName("prodId")]
    public string ProdId { get; set; } = string.Empty;
}

public class WarehouseWorkLinesQuery
{
    [JsonPropertyName("filter")]
    public string Filter { get; set; } = string.Empty;

    [JsonPropertyName("top")]
    public int Top { get; set; }
}

public class ItemBatchesQuery
{
    [JsonPropertyName("filter")]
    public string Filter { get; set; } = string.Empty;

    [JsonPropertyName("top")]
    public int Top { get; set; }
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
/// TSI_Item entity - Item master data for MES operations
/// </summary>
public class TSI_Item
{
    [JsonPropertyName("dataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;

    [JsonPropertyName("ItemId")]
    public string ItemId { get; set; } = string.Empty;

    [JsonPropertyName("ItemName")]
    public string ItemName { get; set; } = string.Empty;

    [JsonPropertyName("ItemGroupId")]
    public string ItemGroupId { get; set; } = string.Empty;

    [JsonPropertyName("ABCValue")]
    public string ABCValue { get; set; } = string.Empty;

    [JsonPropertyName("BOMUnitId")]
    public string BOMUnitId { get; set; } = string.Empty;
}

/// <summary>
/// TSI_ProdBOMLine entity - Production BOM lines with item details
/// </summary>
public class TSI_ProdBOMLine
{
    [JsonPropertyName("dataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;

    [JsonPropertyName("ProdId")]
    public string ProdId { get; set; } = string.Empty;

    [JsonPropertyName("ItemId")]
    public string ItemId { get; set; } = string.Empty;

    [JsonPropertyName("LineNum")]
    public int LineNum { get; set; }

    [JsonPropertyName("BOMQty")]
    public decimal BOMQty { get; set; }

    [JsonPropertyName("BOMQtySerie")]
    public decimal BOMQtySerie { get; set; }

    [JsonPropertyName("UnitId")]
    public string UnitId { get; set; } = string.Empty;

    [JsonPropertyName("Position")]
    public string Position { get; set; } = string.Empty;

    [JsonPropertyName("ScrapVar")]
    public decimal ScrapVar { get; set; }

    [JsonPropertyName("ItemName")]
    public string ItemName { get; set; } = string.Empty;

    [JsonPropertyName("ProdFlushingPrincip")]
    public string ProdFlushingPrincip { get; set; } = string.Empty;

    [JsonPropertyName("NameAlias")]
    public string NameAlias { get; set; } = string.Empty;

    [JsonPropertyName("ItemGroupId")]
    public string ItemGroupId { get; set; } = string.Empty;

    [JsonPropertyName("Depth")]
    public decimal Depth { get; set; }

    [JsonPropertyName("Width")]
    public decimal Width { get; set; }

    [JsonPropertyName("Height")]
    public decimal Height { get; set; }

    [JsonPropertyName("TSIShorteningLength")]
    public decimal TSIShorteningLength { get; set; }

    [JsonPropertyName("TSIBlockWidth")]
    public decimal TSIBlockWidth { get; set; }

    [JsonPropertyName("InventLocationId")]
    public string InventLocationId { get; set; } = string.Empty;

    [JsonPropertyName("wMSLocationId")]
    public string WMSLocationId { get; set; } = string.Empty;

    [JsonPropertyName("inventDimId")]
    public string InventDimId { get; set; } = string.Empty;

    [JsonPropertyName("InventTransId")]
    public string InventTransId { get; set; } = string.Empty;

    [JsonPropertyName("InventRefType")]
    public string InventRefType { get; set; } = string.Empty;

    [JsonPropertyName("InventRefId")]
    public string InventRefId { get; set; } = string.Empty;
}

/// <summary>
/// TSI_Label entity - Production label printing data
/// </summary>
public class TSI_Label
{
    [JsonPropertyName("dataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;

    [JsonPropertyName("ProdId")]
    public string ProdId { get; set; } = string.Empty;

    [JsonPropertyName("LabelItemId")]
    public string LabelItemId { get; set; } = string.Empty;

    [JsonPropertyName("LabelProdId")]
    public string LabelProdId { get; set; } = string.Empty;

    [JsonPropertyName("LabelSalesOrder")]
    public string LabelSalesOrder { get; set; } = string.Empty;

    [JsonPropertyName("LabelPurchOrderFormNum")]
    public string LabelPurchOrderFormNum { get; set; } = string.Empty;

    [JsonPropertyName("LabelEAN_Code")]
    public string LabelEAN_Code { get; set; } = string.Empty;

    [JsonPropertyName("LabelMadeIn")]
    public string LabelMadeIn { get; set; } = string.Empty;

    [JsonPropertyName("LabelAddress")]
    public string LabelAddress { get; set; } = string.Empty;

    [JsonPropertyName("LabelAddressCountryRegionId")]
    public string LabelAddressCountryRegionId { get; set; } = string.Empty;

    [JsonPropertyName("LabelSalesAddressCountryRegionId")]
    public string LabelSalesAddressCountryRegionId { get; set; } = string.Empty;

    [JsonPropertyName("LabelConfigId")]
    public string LabelConfigId { get; set; } = string.Empty;

    [JsonPropertyName("LabelDateWeek")]
    public string LabelDateWeek { get; set; } = string.Empty;

    [JsonPropertyName("LabelDeliveryName")]
    public string LabelDeliveryName { get; set; } = string.Empty;

    [JsonPropertyName("LabelDlvTermTempur")]
    public string LabelDlvTermTempur { get; set; } = string.Empty;

    [JsonPropertyName("LabelItemFreeTxt")]
    public string LabelItemFreeTxt { get; set; } = string.Empty;

    [JsonPropertyName("LabelExternalItemId")]
    public string LabelExternalItemId { get; set; } = string.Empty;

    [JsonPropertyName("UDI")]
    public string UDI { get; set; } = string.Empty;

    [JsonPropertyName("UDIUnit")]
    public string UDIUnit { get; set; } = string.Empty;

    [JsonPropertyName("UDIRefRecId")]
    public long UDIRefRecId { get; set; }

    [JsonPropertyName("HasUDI")]
    public int HasUDI { get; set; }

    [JsonPropertyName("Logos")]
    public List<TSI_LabelLogo>? Logos { get; set; }
}

/// <summary>
/// TSI_LabelLogo entity - Logo selection for labels
/// </summary>
public class TSI_LabelLogo
{
    [JsonPropertyName("dataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;

    [JsonPropertyName("ProdId")]
    public string ProdId { get; set; } = string.Empty;

    [JsonPropertyName("TSILogoId")]
    public string TSILogoId { get; set; } = string.Empty;

    [JsonPropertyName("TSILogoPath")]
    public string TSILogoPath { get; set; } = string.Empty;

    [JsonPropertyName("TSILogoPosition")]
    public int TSILogoPosition { get; set; }

    [JsonPropertyName("TSILogIdDescr")]
    public string TSILogIdDescr { get; set; } = string.Empty;
}

/// <summary>
/// TSI_Job entity - Production jobs data
/// </summary>
public class TSI_Job
{
    [JsonPropertyName("dataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;

    [JsonPropertyName("ProdId")]
    public string ProdId { get; set; } = string.Empty;

    [JsonPropertyName("ModuleRefId")]
    public string ModuleRefId { get; set; } = string.Empty;

    [JsonPropertyName("ItemId")]
    public string ItemId { get; set; } = string.Empty;

    [JsonPropertyName("JobId")]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyName("ItemName")]
    public string ItemName { get; set; } = string.Empty;

    [JsonPropertyName("NameAlias")]
    public string NameAlias { get; set; } = string.Empty;

    [JsonPropertyName("DlvDateProd")]
    public string DlvDateProd { get; set; } = string.Empty;

    [JsonPropertyName("OprNum")]
    public int OprNum { get; set; }

    [JsonPropertyName("Qty")]
    public decimal Qty { get; set; }

    [JsonPropertyName("Height")]
    public decimal Height { get; set; }

    [JsonPropertyName("Width")]
    public decimal Width { get; set; }

    [JsonPropertyName("Depth")]
    public decimal Depth { get; set; }

    [JsonPropertyName("BlockWidth")]
    public decimal BlockWidth { get; set; }

    [JsonPropertyName("Resource")]
    public string Resource { get; set; } = string.Empty;

    [JsonPropertyName("ProdPrioText")]
    public int ProdPrioText { get; set; }

    [JsonPropertyName("TSIPuljeID")]
    public int TSIPuljeID { get; set; }

    [JsonPropertyName("InventBatchId")]
    public string InventBatchId { get; set; } = string.Empty;

    [JsonPropertyName("GreenHandNote")]
    public string GreenHandNote { get; set; } = string.Empty;

    [JsonPropertyName("ItemNameConsumption")]
    public string ItemNameConsumption { get; set; } = string.Empty;

    [JsonPropertyName("StandardPalletQuantity")]
    public string StandardPalletQuantity { get; set; } = string.Empty;

    [JsonPropertyName("TSIShorteningLength")]
    public decimal TSIShorteningLength { get; set; }

    [JsonPropertyName("ProdStatus")]
    public string ProdStatus { get; set; } = string.Empty;

    [JsonPropertyName("TSIReadyForMes")]
    public string TSIReadyForMes { get; set; } = string.Empty;
}

/// <summary>
/// Warehouse Work Lines entity - warehouse operations and work tracking
/// </summary>
public class WarehouseWorkLines
{
    [JsonPropertyName("WorkId")]
    public string WorkId { get; set; } = string.Empty;

    [JsonPropertyName("LineNumber")]
    public int LineNumber { get; set; }

    [JsonPropertyName("ItemNumber")]
    public string ItemNumber { get; set; } = string.Empty;

    [JsonPropertyName("WorkQuantity")]
    public decimal WorkQuantity { get; set; }

    [JsonPropertyName("WorkUnitSymbol")]
    public string WorkUnitSymbol { get; set; } = string.Empty;

    [JsonPropertyName("WMSLocationId")]
    public string WMSLocationId { get; set; } = string.Empty;

    [JsonPropertyName("WarehouseWorkStatus")]
    public string WarehouseWorkStatus { get; set; } = string.Empty;

    [JsonPropertyName("LicensePlateNumber")]
    public string LicensePlateNumber { get; set; } = string.Empty;

    [JsonPropertyName("dataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;
}

/// <summary>
/// Item Batches entity - batch tracking and quarantine management
/// </summary>
public class ItemBatches
{
    [JsonPropertyName("ItemNumber")]
    public string ItemNumber { get; set; } = string.Empty;

    [JsonPropertyName("BatchNumber")]
    public string BatchNumber { get; set; } = string.Empty;

    [JsonPropertyName("BatchDispositionCode")]
    public string BatchDispositionCode { get; set; } = string.Empty;

    [JsonPropertyName("ManufacturingDate")]
    public string ManufacturingDate { get; set; } = string.Empty;

    [JsonPropertyName("BatchExpirationDate")]
    public string BatchExpirationDate { get; set; } = string.Empty;

    [JsonPropertyName("PhysicalInventoryQuantity")]
    public decimal PhysicalInventoryQuantity { get; set; }

    [JsonPropertyName("ReservedInventoryQuantity")]
    public decimal ReservedInventoryQuantity { get; set; }

    [JsonPropertyName("dataAreaId")]
    public string DataAreaId { get; set; } = string.Empty;
}


