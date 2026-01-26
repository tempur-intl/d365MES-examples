# TSI_JmgJobEntity - Core Job Entity Definition

## Overview

**Entity Name**: `TSI_JmgJobEntity`
**Purpose**: Core job data for MES system tracking of active jobs
**Primary Use**: Fast, frequently accessed job information

## Data Sources

### 1. JmgTermReg (Primary Data Source)
- **Join Type**: Primary
- **Filter**: `JobActive = 1`
- Job terminal registration records

### 2. JmgJobTable
- **Join Type**: Inner Join
- **Join Condition**: `JmgTermReg.JobId == JmgJobTable.JobId`
- Job details and operations

### 3. ProdRouteJob
- **Join Type**: Inner Join
- **Join Condition**: `JmgJobTable.JobId == ProdRouteJob.JobId`
- Production route jobs

### 4. ProdTable
- **Join Type**: Inner Join
- **Join Condition**: `ProdRouteJob.ProdId == ProdTable.ProdId`
- Production order header information

### 5. InventTable
- **Join Type**: Inner Join
- **Join Condition**: `JmgJobTable.ItemId == InventTable.ItemId`
- Item master data

### 6. InventDim
- **Join Type**: Inner Join
- **Join Condition**: `ProdTable.InventDimId == InventDim.InventDimId`
- Inventory dimensions (batch, config, etc.)

### 7. EcoResProductTranslation
- **Join Type**: Inner Join
- **Join Condition**: `InventTable.Product == EcoResProductTranslation.Product`
- Product translations for item names

### 8. UnitSeqGroupLine
- **Join Type**: Left Outer Join
- **Join Condition**: `InventTable.UnitSeqGroupId == UnitSeqGroupLine.UnitSeqGroupId`
- **Filter**: `UnitSeqGroupLine.LineNum == 3`
- Unit sequence group for pallet quantities (always Line 3)

## Field Mappings

| Field Name | Data Type | Source Table | Source Field | Mandatory | Description |
|-----------|-----------|--------------|--------------|-----------|-------------|
| `RecId` | Int64 | `JmgTermReg` | `RecId` | Yes | Primary key |
| `ModuleRefId` | String (20) | `JmgJobTable` | `ModuleRefId` | Yes | Production order reference |
| `ItemId` | String (20) | `JmgJobTable` | `ItemId` | Yes | Item identifier |
| `JobId` | String (20) | `JmgTermReg` | `JobId` | Yes | Job identifier |
| `EmplId` | String (20) | `JmgTermReg` | `EmplId` | Yes | Employee ID |
| `StartItems` | Real | `JmgTermReg` | `StartItems` | Yes | Start quantity |
| `RegDateTime` | DateTime | `JmgTermReg` | `RegDateTime` | Yes | Registration timestamp |
| `ItemName` | String (60) | `EcoResProductTranslation` | `Name` | Yes | Item name |
| `ItemNameAlias` | String (60) | `InventTable` | `NameAlias` | Yes | Item alias |
| `DlvDateProd` | Date | `ProdTable` | `DlvDate` | Yes | Production delivery date |
| `OprNum` | String (10) | `JmgJobTable` | `OprNum` | Yes | Operation number |
| `InventBatchId` | String (20) | `InventDim` | `InventBatchId` | Yes | Batch identifier |
| `Height` | Real | `InventTable` | `Height` | Yes | Item height |
| `Width` | Real | `InventTable` | `Width` | Yes | Item width |
| `Depth` | Real | `InventTable` | `Depth` | Yes | Item depth/length |
| `BlockWidth` | Real | `TSIInventTable` | `TSIBlockWidth` | Yes | Block width (custom field) |
| `ProdPrioText` | String (10) | `ProdTable` | `ProdPrio` | Yes | Production priority |
| `TSIPuljeID` | Integer | `ProdTable` | `TSIPuljeID` | Yes | Pulje ID (custom field) |
| `GreenHandNote` | String | `JmgJobTable` | `GreenHandNote` | Yes | Green hand note (custom field) |
| `ItemNameConsumption` | String | `JmgJobTable` | `ItemNameConsumption` | Yes | Consumption text (custom field) |
| `StandardPalletQuantity` | Real | `UnitSeqGroupLine` | `Qty` | Yes | Standard pallet quantity (from unit sequence group line 3) |
| `InventShorteningLength` | Real | `TSIProdTable` | `TSIShorteningLength` | Yes | Shortening length (custom field) |
| `dataAreaId` | String (4) | `JmgTermReg` | `dataAreaId` | Yes | Company identifier |

## Query Filters

### Range on Primary Data Source
```xpp
// On JmgTermReg data source
JobActive == 1
```

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
- **StandardPalletQuantity** is retrieved from `UnitSeqGroupLine` where `LineNum == 3`
- Ensure items have unit sequence groups configured with Line 3 for pallet quantities
- This replaces the deprecated `InventTable.StandardPalletQuantity` field

## Entity Properties

- **Public**: Yes
- **Allow Edit**: No (Read-only entity)
- **Data Management Enabled**: Yes
- **OData Enabled**: Yes
- **Primary Key**: RecId
- **Primary Index**: RecId

## Security

### Privileges Required
- Read access to `JmgTermReg`
- Read access to `JmgJobTable`
- Read access to `ProdTable`
- Read access to `InventTable`
- Read access to `InventDim`
- Read access to `EcoResProductTranslation`

## Performance Considerations

### Recommended Indexes
```sql
-- On JmgTermReg
CREATE INDEX IX_JmgTermReg_JobActive_DataArea
  ON JmgTermReg(JobActive, DataAreaId)
  INCLUDE (RecId, JobId, EmplId);

-- On JmgJobTable
CREATE INDEX IX_JmgJobTable_JobId
  ON JmgJobTable(JobId)
  INCLUDE (ItemId, ModuleRefId, OprNum);

-- On ProdTable
CREATE INDEX IX_ProdTable_ProdId
  ON ProdTable(ProdId)
  INCLUDE (DlvDate, ProdPrio);
```

## OData Endpoint

```
GET /data/TSI_JmgJobs?$filter=dataAreaId eq '500' and JobActive eq true
```

### Example Query - Jobs for Specific Employee
```
GET /data/TSI_JmgJobs?$filter=dataAreaId eq '500' and EmplId eq 'EMP001'
```

### Example Query - Jobs for Specific Item
```
GET /data/TSI_JmgJobs?$filter=dataAreaId eq '500' and ItemId eq 'ITEM123'
```

## TypeScript Interface

```typescript
export interface TSI_JmgJob {
  RecId: number;
  ModuleRefId: string;
  ItemId: string;
  JobId: string;
  EmplId: string;
  StartItems: number;
  RegDateTime: string;
  ItemName: string;
  ItemNameAlias?: string;
  DlvDateProd?: string;
  OprNum: string;
  InventBatchId?: string;
  Height?: number;
  Width?: number;
  Depth?: number;
  BlockWidth?: number;
  ProdPrioText?: string;
  TSIPuljeID?: number;
  GreenHandNote?: string;
  ItemNameConsumption?: string;
  StandardPalletQuantity?: number;
  InventShorteningLength?: number;
  dataAreaId: string;
}
```

## Implementation Checklist

- [ ] Verify all custom fields exist in D365
- [ ] Create entity in D365
- [ ] Add all data sources with proper join types
- [ ] Configure join conditions
- [ ] Add query range filter (JobActive = 1)
- [ ] Map all fields from source tables
- [ ] Set entity properties (Public, OData enabled, etc.)
- [ ] Build and synchronize
- [ ] Grant security privileges
- [ ] Test OData endpoint
- [ ] Performance test with production data volume

## Testing Queries

### Verify Active Jobs in SQL
```sql
SELECT * FROM JmgTermReg
WHERE JobActive = 1
  AND DataAreaId = '500'
```

### Verify Join Logic
```sql
SELECT
  jtr.RecId,
  jtr.JobId,
  jtr.EmplId,
  jjt.ItemId,
  jjt.ModuleRefId,
  pt.ProdId,
  it.NameAlias
FROM JmgTermReg jtr
INNER JOIN JmgJobTable jjt ON jtr.JobId = jjt.JobId
INNER JOIN ProdRouteJob prj ON jjt.JobId = prj.JobId
INNER JOIN ProdTable pt ON prj.ProdId = pt.ProdId
INNER JOIN InventTable it ON jjt.ItemId = it.ItemId
WHERE jtr.JobActive = 1
  AND jtr.DataAreaId = '500'
```

## Notes

- This is a read-only entity for querying active jobs
- Performance is critical - ensure indexes are in place
- Entity is optimized for fast, frequent access
- For label printing data, use separate TSI_JmgJobLabelEntity
