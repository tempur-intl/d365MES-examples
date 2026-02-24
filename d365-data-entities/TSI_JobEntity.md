# TSI_JobEntity - Production Jobs Entity for MES

## Overview

**Entity Name**: `TSI_JobEntity`
**Purpose**: Production job data for MES system integration
**Primary Use**: Job information for production orders with MES integration
**Filtering**: Jobs are filtered to Priority = Primary and Type = Process to include work center information.

## Data Sources

### 1. ProdTable (Primary Data Source)
- **Join Type**: Primary
- Production order header information

### 2. JmgJobTable
- **Join Type**: Outer Join
- **Join Condition**: `ProdTable.ProdId == JmgJobTable.ModuleRefId`
- **Ranges**: `Priority = Primary` and `Type = Process`
- Job details and operations

### 3. InventDim
- **Join Type**: Inner Join
- **Join Condition**: `ProdTable.InventDimId == InventDim.InventDimId`
- Inventory dimensions (batch, config, etc.)

### 4. TSIProdTable
- **Join Type**: Inner Join
- **Join Condition**: `ProdTable.ProdId == TSIProdTable.TSIProdId`
- Custom production table extensions

### 5. InventTable
- **Join Type**: Inner Join
- **Join Condition**: `ProdTable.ItemId == InventTable.ItemId`
- Item master data

### 6. EcoResProductSystemLanguage
- **Join Type**: Outer Join
- **Join Condition**: `InventTable.Product == EcoResProductSystemLanguage.Product`
- Product system language

### 7. EcoResProductTranslation
- **Join Type**: Outer Join
- **Join Condition**: `InventTable.Product == EcoResProductTranslation.Product AND EcoResProductSystemLanguage.LanguageId == EcoResProductTranslation.SystemLanguageId`
- Product translations for item names

### 8. TSIInventTable
- **Join Type**: Outer Join
- **Join Condition**: `InventTable.ItemId == TSIInventTable.TSIItemId`
- Custom inventory table extensions

### 9. WHSInventTable
- **Join Type**: Outer Join
- **Join Condition**: `InventTable.ItemId == WHSInventTable.ItemId`
- Warehouse inventory table

### 10. WHSUOMSeqGroupLine
- **Join Type**: Outer Join
- **Join Condition**: `WHSInventTable.UOMSeqGroupId == WHSUOMSeqGroupLine.UOMSeqGroupId`
- **Filter**: `WHSUOMSeqGroupLine.LineNum == 3`
- Unit sequence group for pallet quantities (always Line 3)

## Field Mappings

| Field Name | Data Type | Source Table | Source Field | Mandatory | Description |
|-----------|-----------|--------------|--------------|-----------|-------------|
| `RecId` | Int64 | `ProdTable` | `RecId` | Yes | Primary key |
| `ProdId` | String (20) | `ProdTable` | `ProdId` | Yes | Production order ID |
| `ModuleRefId` | String (20) | `JmgJobTable` | `ModuleRefId` | No | Production order reference |
| `ItemId` | String (20) | `InventTable` | `ItemId` | Yes | Item identifier |
| `JobId` | String (20) | `JmgJobTable` | `JobId` | No | Job identifier |
| `ItemName` | String (60) | `EcoResProductTranslation` | `Name` | Yes | Item name |
| `NameAlias` | String (60) | `InventTable` | `NameAlias` | No | Item alias |
| `DlvDateProd` | Date | `ProdTable` | `DlvDate` | No | Production delivery date |
| `OprNum` | String (10) | `JmgJobTable` | `OprNum` | No | Operation number |
| `Height` | Real | `InventTable` | `Height` | No | Item height |
| `Width` | Real | `InventTable` | `Width` | No | Item width |
| `Depth` | Real | `InventTable` | `Depth` | No | Item depth/length |
| `BlockWidth` | Real | `TSIInventTable` | `TSIBlockWidth` | No | Block width (custom field) |
| `ProdPrioText` | String (10) | `ProdTable` | `ProdPrio` | No | Production priority |
| `TSIPuljeID` | Integer | `ProdTable` | `TSIPuljeID` | No | Pulje ID (custom field) |
| `GreenHandNote` | String | Computed | `greenHandNote()` | No | Green hand note (computed field) |
| `ItemNameConsumption` | String | Computed | `itemNameConsumption()` | No | Consumption text (computed field) |
| `StandardPalletQuantity` | String | `WHSUOMSeqGroupLine` | `UnitId` | No | Standard pallet quantity (from unit sequence group lineno == 3) |
| `TSIShorteningLength` | Real | `TSIProdTable` | `TSIShorteningLength` | No | Shortening length (custom field) |
| `ProdStatus` | Enum | `ProdTable` | `ProdStatus` | No | Production status |
| `TSIReadyForMes` | Enum | `ProdTable` | `TSIReadyForMes` | No | Ready for MES flag |
| `dataAreaId` | String (4) | `ProdTable` | `dataAreaId` | Yes | Company identifier |

## Computed Fields

### ItemNameConsumption
Retrieves the first consumption item from the production BOM that is associated with the current job.

**Method**: `itemNameConsumption()`
**Returns**: The item name of the first consumption item from the related ProdBOM record. Returns an empty string if no BOM line is found.

**Logic**:
- Looks for the BOM line with the lowest Position value
- If no record is found with Position, falls back to the first line ordered by LineNum
- Joins ProdBOM, InventTable, and EcoResProductTranslation

### GreenHandNote
Retrieves concatenated notes from the DocuRef table associated with the job record.

**Method**: `greenHandNote()`
**Returns**: Concatenated notes separated by " / " from DocuRef records linked to the JmgJobTable record.

**Logic**:
- Queries DocuRef table where RefRecId = JmgJobTable.RecId
- Filters by RefCompanyId = dataAreaId and RefTableId = JmgJobTable table ID
- Orders by CreatedDateTime and concatenates Notes fields

A query range is applied at the entity level to filter jobs with JobStatus = Started. Additional filtering is based on the joined data sources.

## Custom Fields Verification Required

The following fields are custom extensions and must be verified to exist in your D365 environment:

### ProdTable Custom Fields
- `TSIPuljeID`

### TSIProdTable Custom Fields
- `TSIShorteningLength`

### TSIInventTable Custom Fields
- `TSIBlockWidth`

### JmgJobTable Custom Fields
- `ItemNameConsumption`
- `GreenHandNote`

### Unit Sequence Group Configuration
- **StandardPalletQuantity** is retrieved from `WHSUOMSeqGroupLine` where `LineNum == 3`
- Ensure items have unit sequence groups configured with Line 3 for pallet quantities
- This replaces the deprecated `InventTable.StandardPalletQuantity` field

## Entity Properties

- **Public**: Yes
- **Allow Edit**: No (Read-only entity)
- **Data Management Enabled**: Yes
- **OData Enabled**: Yes
- **Primary Key**: EntityKey (RecId)
- **Primary Company Context**: DataAreaId
- **Public Collection Name**: TSI_Jobs
- **Public Entity Name**: TSI_Job

## Security

### Privileges Required
- Read access to `ProdTable`
- Read access to `JmgJobTable`
- Read access to `InventTable`
- Read access to `InventDim`
- Read access to `EcoResProductTranslation`

## Performance Considerations

### Recommended Indexes
```sql
-- On ProdTable
CREATE INDEX IX_ProdTable_ProdId_DataArea
  ON ProdTable(ProdId, DataAreaId)
  INCLUDE (DlvDate, ProdPrio, ItemId, InventDimId);

-- On JmgJobTable
CREATE INDEX IX_JmgJobTable_ModuleRefId
  ON JmgJobTable(ModuleRefId)
  INCLUDE (JobId, OprNum);
```

## OData Endpoint

```
GET /data/TSI_Jobs?$filter=dataAreaId eq '500'
```

### Example Query - Jobs for Specific Production Order
```
GET /data/TSI_Jobs?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'
```

### Example Query - Jobs for Specific Item
```
GET /data/TSI_Jobs?$filter=dataAreaId eq '500' and ItemId eq 'ITEM123'
```

**Response Structure:**
```json
{
  "RecId": 123456,
  "ProdId": "PROD-001234",
  "ItemId": "ITEM123",
  "JobId": "JOB-12345",
  "ItemName": "Pillow Product",
  "ModuleRefId": "PROD-001234",
  "OprNum": "10",
  "dataAreaId": "500"
}
```

**For Label Printing Data:**
Query TSI_LabelEntity separately using the ProdId:
```
GET /data/TSI_Labels?$filter=ProdId eq '{ProdId}'&$expand=Logos
```

## TypeScript Interface

```typescript
export interface TSI_Job {
  RecId: number;
  ProdId: string;
  ModuleRefId?: string;
  ItemId: string;
  JobId?: string;
  ItemName: string;
  NameAlias?: string;
  DlvDateProd?: string;
  OprNum?: string;
  InventBatchId?: string;
  Height?: number;
  Width?: number;
  Depth?: number;
  BlockWidth?: number;
  ProdPrioText?: string;
  TSIPuljeID?: number;
  GreenHandNote?: string;
  ItemNameConsumption?: string;
  StandardPalletQuantity?: string;
  TSIShorteningLength?: number;
  ProdStatus?: number;
  TSIReadyForMes?: number;
  dataAreaId: string;
}
```

## Implementation Checklist

- [ ] Verify all custom fields exist in D365
- [ ] Create entity in D365
- [ ] Add all data sources with proper join types
- [ ] Configure join conditions
- [ ] Add query range filters (Priority = Primary and Type = Process) on JmgJobTable
- [ ] Map all fields from source tables
- [ ] Set entity properties (Public, OData enabled, etc.)
- [ ] Build and synchronize
- [ ] Grant security privileges
- [ ] Test OData endpoint
- [ ] Performance test with production data volume

## Testing Queries

## Testing Queries

### Verify Production Orders with Jobs
```sql
SELECT pt.ProdId, pt.ItemId, jt.JobId, jt.OprNum
FROM ProdTable pt
LEFT OUTER JOIN JmgJobTable jt ON pt.ProdId = jt.ModuleRefId
WHERE pt.DataAreaId = '500'
```

### Verify Join Logic
```sql
SELECT
  pt.RecId,
  pt.ProdId,
  jt.ModuleRefId,
  it.ItemId,
  jt.JobId,
  et.Name AS ItemName
FROM ProdTable pt
LEFT OUTER JOIN JmgJobTable jt ON pt.ProdId = jt.ModuleRefId
INNER JOIN InventTable it ON pt.ItemId = it.ItemId
LEFT OUTER JOIN EcoResProductTranslation et ON it.Product = et.Product
WHERE pt.DataAreaId = '500'
```

## Notes

- This is a read-only entity for querying production jobs
- Performance is critical - ensure indexes are in place
- Entity is optimized for MES integration
- **MES Integration**: For label printing, query TSI_LabelEntity separately using ProdId from this entity
- Use: `GET /data/TSI_Labels?$filter=ProdId eq '{ProdId}'&$expand=Logos` to get label data with logos
- Jobs and Labels are separate queries - no navigation property between them
