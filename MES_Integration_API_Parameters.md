# MES Integration API Parameters Documentation

This document provides comprehensive mapping of parameters used by Manufacturing Execution Systems (MES) when calling the Dynamics 365 Supply Chain Management MES Integration API.

## Overview

The MES Integration API enables third-party MES systems to communicate production events to D365 Supply Chain Management. The API supports four standard message types plus two TSI custom message types:

**Standard message types:**
1. **Start Production Order** (`ProdProductionOrderStart`)
2. **Report as Finished** (`ProdProductionOrderReportFinished`)
3. **Material Consumption** (`ProdProductionOrderPickingList`)
4. **End Production Order** (`ProdProductionOrderEnd`)

**TSI custom message types:**
5. **Inventory Count Journal** (`TSIInventCountJournal`)
6. **Update Batch Disposition** (`TSIUpdateBatchDisposition`)

## API Endpoint

```
POST /api/services/SysMessageServices/SysMessageService/SendMessage
```

## Message Envelope Structure

All MES Integration API messages follow this envelope structure:

```json
{
    "_companyId": "USMF",
    "_messageQueue": "JmgMES3P",
    "_messageType": "ProdProductionOrderStart",
    "_messageContent": "{...}"
}
```

### Envelope Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `_companyId` | String | Yes | Legal entity/company identifier in D365 |
| `_messageQueue` | String | No | Message queue identifier (default: "JmgMES3P") |
| `_messageType` | String | Yes | Type of MES message being sent |
| `_messageContent` | String | Yes | JSON payload containing the message data |

## 1. Start Production Order Message

**Message Type:** `ProdProductionOrderStart`

### Parameters

| Parameter | Type | Required | Default Value |
|-----------|------|----------|---------------|
| `ProductionOrderNumber` | String | Mandatory | N/A |
| `StartedQuantity` | Real | Optional | Production quantity (e.g., 50,00) |
| `StartedDate` | Date | Optional | Current date |
| `AutomaticBOMConsumptionRule` | Enum (FlushingPrincip \| Always \| Never) | Optional | Never |
| `AutomaticRouteConsumptionRule` | Enum (RouteDependent \| Always \| Never) | Optional | Never |

### Example

```json
{
    "_companyId": "USMF",
    "_messageType": "ProdProductionOrderStart",
    "_messageContent": "{\"ProductionOrderNumber\": \"P000123\", \"StartedQuantity\": 100, \"StartedDate\": \"2024-01-15\", \"AutomaticBOMConsumptionRule\": \"FlushingPrincip\", \"AutomaticRouteConsumptionRule\": \"Always\"}"
}
```

## 2. Report as Finished Message

**Message Type:** `ProdProductionOrderReportFinished`

### Main Parameters

| Parameter | Type | Required | Default Value |
|-----------|------|----------|---------------|
| `ProductionOrderNumber` | String | Mandatory | N/A |
| `ReportFinishedLines` | A list of lines | Mandatory | N/A |
| `PrintLabel` | Enum (Yes \| No) | Optional | N/A |

### ReportFinishedLines Parameters

| Parameter | Type | Required | Default Value |
|-----------|------|----------|---------------|
| `LineNumber` | Real | Optional | N/A |
| `ItemNumber` | String | Optional | N/A |
| `ProductionType` | Enum (MainItem \| Formula \| BOM \| Co_Product \| By_Product \| None), extensible | Optional | N/A |
| `ReportedErrorQuantity` | Real | Optional | (blank) |
| `ReportedGoodQuantity` | Real | Optional | Production quantity (e.g., 50,00) |
| `ReportedErrorCatchWeightQuantity` | Real | Optional | N/A |
| `ReportedGoodCatchWeightQuantity` | Real | Optional | N/A |
| `AcceptError` | Enum (Yes \| No) | Optional | No |
| `ErrorCause` | Enum (None \| Material \| Machine \| OperatingStaff), extensible | Optional | (blank) |
| `ExecutedDateTime` | DateTime | Optional | N/A |
| `ReportAsFinishedDate` | Date | Optional | Current date |
| `AutomaticBOMConsumptionRule` | Enum (FlushingPrincip \| Always \| Never) | Optional | FlushingPrincip |
| `AutomaticRouteConsumptionRule` | Enum (RouteDependent \| Always \| Never) | Optional | Always |
| `RespectFlushingPrincipleDuringOverproduction` | Enum (Yes \| No) | Optional | N/A |
| `JournalNameId` | String | Optional | RAF |
| `PickingListJournalNameId` | String | Optional | Pick |
| `RouteCardJournalNameId` | String | Optional | Route |
| `FromOperationNumber` | Integer | Optional | (blank) |
| `ToOperationNumber` | Integer | Optional | (blank) |
| `InventoryLotId` | String | Optional | N/A |
| `BaseValue` | String | Optional | N/A |
| `EndJob` | Enum (Yes \| No) | Optional | No |
| `EndPickingList` | Enum (Yes \| No) | Optional | No |
| `EndRouteCard` | Enum (Yes \| No) | Optional | No |
| `PostNow` | Enum (Yes \| No) | Optional | N/A |
| `AutoUpdate` | Enum (Yes \| No) | Optional | N/A |
| `ProductColorId` | String | Optional | N/A |
| `ProductConfigurationId` | String | Optional | N/A |
| `ProductSizeId` | String | Optional | N/A |
| `ProductStyleId` | String | Optional | N/A |
| `ProductVersionId` | String | Optional | N/A |
| `ItemBatchNumber` | String | Optional | N/A |
| `ProductSerialNumber` | String | Optional | N/A |
| `GenerateLicensePlate` | Enum (Yes \| No) | Optional | N/A |
| `LicensePlateNumber` | String | Optional | N/A |
| `InventoryStatusId` | String | Optional | N/A |
| `ProductionWarehouseId` | String | Optional | N/A |
| `ProductionSiteId` | String | Optional | N/A |
| `ProductionWarehouseLocationId` | String | Optional | N/A |
| `InventoryDimension1` | String | Optional | N/A |
| `InventoryDimension2` | String | Optional | N/A |
| `InventoryDimension3` | String | Optional | N/A |
| `InventoryDimension4` | String | Optional | N/A |
| `InventoryDimension5` | String | Optional | N/A |
| `InventoryDimension6` | String | Optional | N/A |
| `InventoryDimension7` | String | Optional | N/A |
| `InventoryDimension8` | String | Optional | N/A |
| `InventoryDimension9` | String | Optional | N/A |
| `InventoryDimension10` | String | Optional | N/A |
| `InventoryDimension11` | String | Optional | N/A |
| `InventoryDimension12` | String | Optional | N/A |

## 3. Material Consumption (Picking List) Message

**Message Type:** `ProdProductionOrderPickingList`

### Main Parameters

| Parameter | Type | Required |
|-----------|------|----------|
| `ProductionOrderNumber` | String | Mandatory |
| `JournalNameId` | String | Optional |
| `PickingListLines` | A list of lines | Mandatory |

### PickingListLines Parameters

| Parameter | Type | Required |
|-----------|------|----------|
| `ItemNumber` | String | Mandatory |
| `ConsumptionBOMQuantity` | Real | Optional |
| `ProposalBOMQuantity` | Real | Optional |
| `ScrapBOMQuantity` | Real | Optional |
| `BOMUnitSymbol` | String | Optional |
| `ConsumptionInventoryQuantity` | Real | Optional |
| `ProposalInventoryQuantity` | Real | Optional |
| `ConsumptionCatchWeightQuantity` | Real | Optional |
| `ProposalCatchWeightQuantity` | Real | Optional |
| `ConsumptionDate` | Date | Optional |
| `OperationNumber` | Integer | Optional |
| `LineNumber` | Real | Optional |
| `PositionNumber` | String | Optional |
| `IsConsumptionEnded` | Enum (Yes \| No) | Optional |
| `ErrorCause` | Enum (None \| Material \| Machine \| OperatingStaff), extensible | Optional |
| `InventoryLotId` | String | Optional |
| `ProductColorId` | String | Optional |
| `ProductConfigurationId` | String | Optional |
| `ProductSizeId` | String | Optional |
| `ProductStyleId` | String | Optional |
| `ProductVersionId` | String | Optional |
| `ItemBatchNumber` | String | Optional |
| `ProductSerialNumber` | String | Optional |
| `LicensePlateNumber` | String | Optional |
| `InventoryStatusId` | String | Optional |
| `ProductionWarehouseId` | String | Optional |
| `ProductionSiteId` | String | Optional |
| `ProductionWarehouseLocationId` | String | Optional |
| `InventoryDimension1` | String | Optional |
| `InventoryDimension2` | String | Optional |
| `InventoryDimension3` | String | Optional |
| `InventoryDimension4` | String | Optional |
| `InventoryDimension5` | String | Optional |
| `InventoryDimension6` | String | Optional |
| `InventoryDimension7` | String | Optional |
| `InventoryDimension8` | String | Optional |
| `InventoryDimension9` | String | Optional |
| `InventoryDimension10` | String | Optional |
| `InventoryDimension11` | String | Optional |
| `InventoryDimension12` | String | Optional |

## 4. End Production Order Message

**Message Type:** `ProdProductionOrderEnd`

### Parameters

| Parameter | Type | Required |
|-----------|------|----------|
| `ProductionOrderNumber` | String | Mandatory |
| `ExecutedDateTime` | DateTime | Optional |
| `EndedDate` | Date | Optional |
| `UseTimeAndAttendanceCost` | Enum (Yes \| No) | Optional |
| `AutoReportAsFinished` | Enum (Yes \| No) | Optional |
| `AutoUpdate` | Enum (Yes \| No) | Optional |

## Note: Route Card Message Not Used

The standard MES integration also defines a fifth message type — **`ProdProductionOrderRouteCard`** (time used for operation / route card). This message type is **not implemented in this integration**.

Reason: D365 route/operation costing is not used for MES-handled production orders. Time tracking, operation sequencing, and routing are managed entirely within the MES and are not written back to D365. If this changes in future, refer to the [Microsoft MES Integration documentation](https://learn.microsoft.com/en-us/dynamics365/supply-chain/production-control/mes-integration#time-used-for-operation-route-card-message) for the full parameter set.

## 5. Inventory Count Journal Message

**Message Type:** `TSIInventCountJournal`

Creates an inventory counting journal line in D365 for a specific item, location, and license plate. Typically sent after a physical count on the production floor.

### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `ProductionOrderNumber` | String | Mandatory | Production order the count is associated with |
| `ItemNumber` | String | Mandatory | Item number being counted |
| `Location` | String | Mandatory | Warehouse location where the count was performed |
| `LicensePlate` | String | Optional | License plate at the counted location |
| `CountedQuantity` | String | Mandatory | Physical count quantity (sent as a string) |
| `CountDate` | Date | Mandatory | Date the physical count was performed (ISO 8601: `yyyy-MM-dd`) |

### Example

```json
{
  "_companyId": "500",
  "_messageQueue": "JmgMES3P",
  "_messageType": "TSIInventCountJournal",
  "_messageContent": "{\"ProductionOrderNumber\":\"10001147\",\"ItemNumber\":\"20821\",\"Location\":\"Aisle01-Rack01-Shelf01\",\"LicensePlate\":\"LP-2024-001\",\"CountedQuantity\":\"10\",\"CountDate\":\"2024-01-15\"}"
}
```

## 6. Update Batch Disposition Message

**Message Type:** `TSIUpdateBatchDisposition`

Updates the disposition code for a specific inventory batch in D365. Used when MES completes a quality inspection or quarantine review and needs to change the batch status (e.g., from `Quarantine` to `Available`).

### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `ProductionOrderNumber` | String | Mandatory | Production order the batch is associated with |
| `ItemNumber` | String | Mandatory | Item number of the batch |
| `BatchNumber` | String | Mandatory | Batch/lot number to update |
| `DispositionCode` | String | Mandatory | Target disposition code as configured in D365 (e.g., `Available`, `Quarantine`) |

### Example

```json
{
  "_companyId": "500",
  "_messageQueue": "JmgMES3P",
  "_messageType": "TSIUpdateBatchDisposition",
  "_messageContent": "{\"ProductionOrderNumber\":\"10001147\",\"ItemNumber\":\"20821\",\"BatchNumber\":\"BATCH001\",\"DispositionCode\":\"Available\"}"
}
```

## Configuration and Setup

### Enabling MES Integration
1. Enable Time and attendance license key in System administration > Setup > License configuration
2. Configure MES integration processes in Production control > Setup > Manufacturing execution > Manufacturing execution systems integration

### Authentication
Use Azure AD OAuth 2.0 authentication with the D365 API scope.

### Monitoring
Monitor message processing in Production control > Setup > Manufacturing execution > Manufacturing execution systems integration

## Error Handling

Messages are processed in sequence and retried up to 3 times on failure. Failed messages can be reviewed and corrected through the Manufacturing execution systems integration page.

## Best Practices

1. **Validate Data**: Ensure all required parameters are provided
2. **Use Appropriate Units**: Match quantity units with D365 configuration
3. **Handle Dimensions**: Include relevant product and inventory dimensions
4. **Date Formats**: Use ISO date formats (YYYY-MM-DD)
5. **Batch Processing**: Send related operations in logical sequence
6. **Error Monitoring**: Set up alerts for failed message processing

## References

- [MES Integration API Documentation](https://learn.microsoft.com/en-us/dynamics365/supply-chain/production-control/mes-integration)
- [Message Processor Documentation](https://learn.microsoft.com/en-us/dynamics365/supply-chain/message-processor/message-processor)
- [Message Processor Developer Guide](https://learn.microsoft.com/en-us/dynamics365/supply-chain/message-processor/developer/message-processor-develop)
- [D365 Data Entities](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/data-entities/data-entities-data-packages)
- [Business Events](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/business-events/home-page)