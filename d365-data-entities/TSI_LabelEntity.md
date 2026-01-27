# TSI_LabelEntity - Job Label Printing Entity Definition

## Overview

**Entity Name**: `TSI_LabelEntity`
**Purpose**: Label printing data for active jobs with sales, customer, and image information
**Primary Use**: MES system label printing with comprehensive order and delivery details

## Data Sources

### 1. JmgTermReg (Primary Data Source)
- **Join Type**: Primary
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

### 7. SalesLine
- **Join Type**: Left Outer Join
- **Join Condition**: `ProdTable.ProdId == SalesLine.InventRefId`
- Sales order line reference

### 8. SalesTable
- **Join Type**: Left Outer Join
- **Join Condition**: `SalesLine.SalesId == SalesTable.SalesId`
- Customer delivery information

### 9. LogisticsPostalAddress
- **Join Type**: Left Outer Join
- **Join Condition**: `SalesTable.DeliveryPostalAddress == LogisticsPostalAddress.RecId`
- Delivery postal address details

### 10. CustTable
- **Join Type**: Left Outer Join
- **Join Condition**: `SalesTable.CustAccount == CustTable.AccountNum`
- Customer details

### 11. DlvTerm
- **Join Type**: Left Outer Join
- **Join Condition**: `SalesTable.DlvTerm == DlvTerm.Code`
- Delivery terms text

### 12. InventItemBarcode
- **Join Type**: Left Outer Join
- **Join Condition**: Complex barcode lookup logic (see Barcode Logic section)
- EAN codes

### 13. TSIUDI
- **Join Type**: Left Outer Join
- **Join Condition**: `ProdTable.ProdId == TSIUDI.ReferenceNumber AND TSIUDI.ReferenceType == TSIReferenceType::ProductionOrder AND ProdTable.ItemId == TSIUDI.ItemId AND InventDim.ConfigId == TSIUDI.ConfigId`
- UDI (Unique Device Identifier) lookup for production orders
- **Note**: Multiple UDI records can exist per production order (one per unit). The entity will return **multiple rows per job** when multiple units exist. Filter by `UDIUnit` on the client side to get a specific unit.

## Field Mappings

| Field Name | Data Type | Source Table | Source Field | Mandatory | Description |
|-----------|-----------|--------------|--------------|-----------|-------------|
| `RecId` | Int64 | `JmgTermReg` | `RecId` | Yes | Primary key (links to TSI_JmgJobEntity) |
| `ProdId` | String (20) | `ProdTable` | `ProdId` | Yes | Production order ID (for filtering) |
| `JobId` | String (20) | `JmgJobTable` | `JobId` | Yes | Job ID (for filtering) |
| `LabelAddress` | String | `LogisticsPostalAddress` | `Address` | No | Delivery address (formatted) |
| `LabelAddressCountryRegionId` | String (10) | `CustTable` | `PartyCountry` | No | Customer country |
| `LabelSalesAddressCountryRegionId` | String (10) | `LogisticsPostalAddress` | `CountryRegionId` | No | Delivery country |
| `LabelConfigId` | String (10) | `InventDim` | `ConfigId` | No | Configuration |
| `LabelDateWeek` | String | Computed | Computed from `SalesLine.ConfirmedDlv` | No | Date and week formatted (see DateWeek Logic) |
| `LabelDeliveryName` | String (60) | `SalesLine` | `DeliveryName` | No | Delivery name |
| `LabelDlvTermTempur` | String | `DlvTerm` | `Txt` | No | Delivery term text |
| `LabelItemFreeTxt` | String (60) | `SalesLine` | `Name` | No | Free text |
| `LabelItemId` | String (20) | `SalesLine` | `ItemId` | No | Sales item ID |
| `LabelProdId` | String (20) | `SalesLine` | `InventRefId` | No | Production ID reference |
| `LabelPurchOrderFormNum` | String (20) | `SalesTable` | `PurchOrderFormNum` | No | PO reference |
| `LabelSalesOrder` | String (20) | `SalesTable` | `SalesId` | No | Sales order number |
| `LabelEAN_Code` | String (20) | Computed | Computed | No | EAN barcode (complex logic) |
| `LabelMadeIn` | String (20) | `InventTable` | `MadeIn` | No | Country of origin (custom field) |
| `LabelExternalItemId` | String (20) | `SalesLine` | `ExternalItemId` | No | External item reference |
| `UDI` | String | `TSIUDI` | `UDI` | No | Unique device ID (from custom TSIUDI table) |
| `UDIUnit` | String | `TSIUDI` | `Unit` | No | Unit for the UDI (e.g., 'x1', 'x10', 'x100') |
| `dataAreaId` | String (4) | `JmgTermReg` | `dataAreaId` | Yes | Company identifier |

## Navigation Properties

### Logos (One-to-Many)
- **Related Entity**: `TSI_LabelLogoEntity`
- **Relationship**: One label can have multiple logos
- **Foreign Key**: `RecId`
- **Navigation Name**: `Logos`
- **Usage**: Use `$expand=Logos` to retrieve applicable logos in the same call

**Example:**
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'&$expand=Logos($orderby=TSILogoPosition)
```

See [TSI_LabelLogoEntity.md](TSI_LabelLogoEntity.md) for logo filtering logic and implementation details.

### Job (Many-to-One)
- **Related Entity**: `TSI_JmgJobEntity`
- **Relationship**: Multiple label records (per UDI unit) belong to one job
- **Foreign Key**: `RecId`
- **Navigation Name**: `Job`
- **Inverse Navigation**: `Label` (from job to labels)

**Recommended Pattern for MES (Label + Logos in One Call):**
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and JobId eq 'JOB-12345'&$expand=Logos
```

**Note**: D365 Finance & Operations only supports **first-level $expand**. Nested expansion like `$expand=Label($expand=Logos)` is NOT supported. Query this entity directly with `$expand=Logos` to get label data and logos together.

### Required Query Parameters

**IMPORTANT**: This entity MUST be called with either `JobId` or `ProdId` filter to prevent full table scans.

**Enforcement**: Implement validation in the entity's `validateRead()` method:

```xpp
public boolean validateRead()
{
    boolean ret;
    QueryBuildDataSource qbds;
    QueryBuildRange qbr;
    boolean hasJobIdFilter = false;
    boolean hasProdIdFilter = false;

    ret = super();

    if (ret)
    {
        // Check for JobId or ProdId filter
        qbds = this.query().dataSourceTable(tableNum(JmgTermReg));

        // Check JobId filter
        qbr = qbds.findRange(fieldNum(JmgJobTable, JobId));
        if (qbr && qbr.value())
        {
            hasJobIdFilter = true;
        }

        // Check ProdId filter
        qbr = qbds.findRange(fieldNum(ProdTable, ProdId));
        if (qbr && qbr.value())
        {
            hasProdIdFilter = true;
        }

        if (!hasJobIdFilter && !hasProdIdFilter)
        {
            error("TSI_LabelEntity must be filtered by JobId or ProdId.");
            ret = false;
        }
    }

    return ret;
}
```

**OData Impact**: Queries without JobId or ProdId filter will return an error:
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500'
// Returns error: "TSI_LabelEntity must be filtered by JobId or ProdId."
```

**Valid Queries**:
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and JobId eq 'JOB-12345'
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'
```

## Barcode Logic (LabelEAN_Code)

The EAN code field has complex lookup logic that needs to be implemented as a computed field or method:

```sql
CAST(
  CASE
    -- If barcode exists for specific config, use it
    WHEN EXISTS(
      SELECT ITEMBARCODE
      FROM InventItemBarcode
      WHERE ItemId = @ItemId AND ConfigId = @ConfigId
    )
    THEN (SELECT ITEMBARCODE WHERE ItemId = @ItemId AND ConfigId = @ConfigId)

    -- Otherwise, use 'Std' config barcode
    ELSE (SELECT ITEMBARCODE WHERE ItemId = @ItemId AND ConfigId = 'Std')
  END AS NVARCHAR
) AS LabelEAN_Code
```

**Implementation Notes:**
- First check if barcode exists for the specific ConfigId from InventDim
- If found, use that barcode
- If not found, fallback to barcode with ConfigId = 'Std'
- This requires a computed field or display method in X++

## DateWeek Logic (LabelDateWeek)

The DateWeek field is computed from `SalesLine.ConfirmedDlv` date:

**Legacy X++ Logic:**
```xpp
getWeek = wkofyr(getDate);  // Get week of year from date
dateAndWeek = Date2Str(getDate, 321, 2, 1, 2, 1, 2) + ' ' + '(' + int2str(getWeek) + ')';
// Result format: "2026-01-26 (4)"
```

**Implementation Notes:**
- Source: `SalesLine.ConfirmedDlv` (confirmed delivery date)
- Calculate week of year using `wkofyr()` function
- Format date as string using `Date2Str()` with format code 321
- Combine as: `"date (week_number)"`
- Example output: `"2026-01-26 (4)"` for week 4 of 2026
- This requires a computed field or display method in X++

## Custom Fields Verification Required

The following fields are custom extensions and must be verified to exist in your D365 environment:

### TSIUDI Custom Table
- Custom table for storing UDI numbers per production/purchase order
- Fields: `ItemId`, `ReferenceType` (enum), `ReferenceNumber`, `ConfigId`, `Unit`, `UDI`
- Join on: `ProdId`, `ItemId`, `ConfigId`, and `ReferenceType = ProductionOrder`

### Logo Setup
- Logos are retrieved via the related `TSI_LabelLogoEntity` (navigation property)
- See [TSI_LabelLogoEntity.md](TSI_LabelLogoEntity.md) for logo setup tables and filtering logic

### InventTable Custom Fields
- `MadeIn`

## Entity Properties

- **Public**: Yes
- **Allow Edit**: No (Read-only entity)
- **Data Management Enabled**: Yes
- **OData Enabled**: Yes
- **Primary Key**: RecId, dataAreaId
- **Primary Index**: RecId

## Security

### Privileges Required
- Read access to `JmgTermReg`
- Read access to `JmgJobTable`
- Read access to `ProdTable`
- Read access to `InventTable`
- Read access to `InventDim`
- Read access to `SalesLine`
- Read access to `SalesTable`
- Read access to `LogisticsPostalAddress`
- Read access to `CustTable`
- Read access to `DlvTerm`
- Read access to `InventItemBarcode`
- Read access to `TSIUDI`
- Execute permission on `TSI_LabelEntity`
- View permission for Sales and Job Management modules

## Performance Considerations

### Recommended Indexes
```sql
-- On SalesLine for production order lookup
CREATE INDEX IX_SalesLine_InventRefId
  ON SalesLine(InventRefId, DataAreaId)
  INCLUDE (SalesId, ItemId, DeliveryName);

-- On InventItemBarcode for barcode lookup
CREATE INDEX IX_InventItemBarcode_ItemId_ConfigId
  ON InventItemBarcode(ItemId, ConfigId)
  INCLUDE (ItemBarCode);
```

### Caching Strategy
Since label data rarely changes during a job:
- Cache label data after first retrieval
- Only query when label printing is needed
- Not needed for every job status check

## OData Endpoint

```
GET /data/TSI_Labels?$filter=dataAreaId eq '500'
```

### Example Query - Get Label Data for Specific Production Order
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'
```

### Example Query - Get Label Data for Specific Job
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and JobId eq 'JOB-12345'
```

### Example Query - Get Label Data with Logos Expanded
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'&$expand=Logos($orderby=TSILogoPosition)
```

### Example Query - Get Label Data with Sales Order
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and LabelSalesOrder ne null
```

### Example Query - Get Label Data for Specific Unit
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234' and UDIUnit eq 'x10'
```

**Note**: Since the entity returns multiple rows per job (one per unit), you should filter by `UDIUnit` to get the specific unit you need for label printing.

## TypeScript Interface

```typescript
export interface TSI_Label {
  RecId: number; // Links to TSI_JmgJob.RecId
  ProdId: string; // Production order ID (for filtering)
  JobId: string; // Job ID (for filtering)
  LabelAddress?: string;
  LabelAddressCountryRegionId?: string;
  LabelSalesAddressCountryRegionId?: string;
  LabelConfigId?: string;
  LabelDateWeek?: string; // Computed: "date (week)" format
  LabelDeliveryName?: string;
  LabelDlvTermTempur?: string;
  LabelItemFreeTxt?: string;
  LabelItemId?: string;
  LabelProdId?: string;
  LabelPurchOrderFormNum?: string;
  LabelSalesOrder?: string;
  LabelEAN_Code?: string;
  LabelMadeIn?: string;
  LabelExternalItemId?: string;
  UDI?: string;
  UDIUnit?: string;
  dataAreaId: string;
  Logos?: TSI_LabelLogo[]; // Navigation property (use $expand=Logos)
}

export interface TSI_LabelLogo {
  RecId: number;
  TSILogoId: string;
  TSILogoPath: string;
  TSILogoPosition: number;
  TSILogIdDescr?: string;
  dataAreaId: string;
}
```

## Implementation Checklist

- [ ] Verify all custom fields exist in D365
- [ ] Create entity in D365
- [ ] Add all data sources with proper join types (LEFT OUTER JOIN for sales-related tables)
- [ ] Configure join conditions
- [ ] Implement barcode lookup logic (computed field or method)
- [ ] Implement DateWeek computation from SalesLine.ConfirmedDlv (computed field or method)
- [ ] Create navigation property relationship to TSI_LabelLogoEntity (Logos)
- [ ] Create navigation property relationship to TSI_JmgJobEntity (Job - inverse of Label)
- [ ] Map all fields from source tables
- [ ] Implement validateRead() method to enforce JobId or ProdId filter
- [ ] Set entity properties (Public, OData enabled, etc.)
- [ ] Build and synchronize
- [ ] Grant security privileges
- [ ] Test OData endpoint
- [ ] Test with jobs that have sales orders
- [ ] Test with jobs that do NOT have sales orders
- [ ] Performance test with production data volume

## Testing Queries

### Verify Jobs with Sales Orders
```sql
SELECT
  jtr.RecId,
  jtr.JobId,
  lpa.Address AS DeliveryAddress,
  lpa.CountryRegionId AS DeliveryCountryRegionId,
  ct.PartyCountry,
  sl.DeliveryName,
  st.SalesId,
  udi.UDI
FROM JmgTermReg jtr
INNER JOIN JmgJobTable jjt ON jtr.JobId = jjt.JobId
INNER JOIN ProdRouteJob prj ON jjt.JobId = prj.JobId
INNER JOIN ProdTable pt ON prj.ProdId = pt.ProdId
INNER JOIN InventDim id ON pt.InventDimId = id.InventDimId
LEFT JOIN SalesLine sl ON pt.ProdId = sl.InventRefId
LEFT JOIN SalesTable st ON sl.SalesId = st.SalesId
LEFT JOIN LogisticsPostalAddress lpa ON st.DeliveryPostalAddress = lpa.RecId
LEFT JOIN CustTable ct ON st.CustAccount = ct.AccountNum
LEFT JOIN TSIUDI udi ON pt.ProdId = udi.ReferenceNumber
  AND udi.ReferenceType = 0  -- TSIReferenceType::ProductionOrder
  AND pt.ItemId = udi.ItemId
  AND id.ConfigId = udi.ConfigId
WHERE jtr.DataAreaId = '500'
  AND st.SalesId IS NOT NULL
```

### Verify Barcode Logic
```sql
-- Test barcode lookup for specific config
SELECT
  ItemId,
  ConfigId,
  ItemBarCode
FROM InventItemBarcode
WHERE ItemId = 'TEST-ITEM'
  AND (ConfigId = 'SpecificConfig' OR ConfigId = 'Std')
ORDER BY
  CASE WHEN ConfigId = 'SpecificConfig' THEN 1 ELSE 2 END
```

## Notes

- This is a read-only entity for querying label data
- Many fields are optional (Left Outer Joins) as not all jobs have sales orders
- Entity is designed for label printing scenarios only
- **MES Integration**: Prefer calling TSI_JmgJobEntity with `$expand=Label($expand=Logos)` for single-call retrieval
- Can be queried directly with JobId or ProdId filter (required)
- Barcode lookup logic requires computed field implementation
- Logo files are retrieved via `$expand=Logos` navigation property (see TSI_LabelLogoEntity)
- Performance: Only query when printing labels, not for every job check

### UDI (Unique Device Identifier) Logic
- UDI numbers are stored in the custom `TSIUDI` table
- Records are created per production order using `TSIUDI::createFromProdTable()`
- Join conditions: Match on `ProdId` (as ReferenceNumber), `ItemId`, `ConfigId`, and `ReferenceType = ProductionOrder`
- **Multiple rows per job**: Entity returns one row per unit when multiple UDI records exist
- **Unit filtering**: MES should filter by `UDIUnit` in the OData query to get the desired unit (e.g., `$filter=UDIUnit eq 'x10'`)
- **Single call support**: All label data and UDI values available in one query, filtered by unit on client side
- UDI is generated using `TSIUDIHelper::generateUDI(ProdId, GTIN)`
- The TSIUDI table supports both production orders and purchase orders via the ReferenceType enum
- UDI records are created with item configuration and unit information from GTIN setup
