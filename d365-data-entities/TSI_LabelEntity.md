# TSI_LabelEntity - Production Label Printing Entity Definition

## Overview

**Entity Name**: `TSI_LabelEntity`
**Purpose**: Label printing data for production orders with sales, customer, and UDI information
**Primary Use**: MES system label printing with comprehensive order and delivery details

## Data Sources

### 1. ProdTable (Primary Data Source)
- **Join Type**: Primary
- Production order header information

### 2. InventTable
- **Join Type**: Inner Join
- **Join Condition**: `ProdTable.ItemId == InventTable.ItemId`
- Item master data

### 3. LogisticsAddressCountryRegion
- **Join Type**: Outer Join
- **Join Condition**: `InventTable.CountryRegionId == LogisticsAddressCountryRegion.OrigCountryRegionId`
- Country/region information for origin

### 4. LogisticsAddressCountryRegionTranslation
- **Join Type**: Outer Join
- **Join Condition**: `LogisticsAddressCountryRegion.CountryRegionId == LogisticsAddressCountryRegionTranslation.CountryRegionId`
- **Filter**: `LanguageId == 'en-us'`
- Translated country names

### 5. InventDim
- **Join Type**: Inner Join
- **Join Condition**: `ProdTable.InventDimId == InventDim.InventDimId`
- Inventory dimensions (batch, config, etc.)

### 6. SalesLine
- **Join Type**: Outer Join
- **Join Condition**: `ProdTable.ProdId == SalesLine.InventRefId`
- **Filter**: `InventRefType == Production`
- Sales order line reference

### 7. SalesTable
- **Join Type**: Outer Join
- **Join Condition**: `SalesLine.SalesId == SalesTable.SalesId`
- Customer delivery information

### 8. LogisticsPostalAddress
- **Join Type**: Outer Join
- **Join Condition**: `SalesTable.DeliveryPostalAddress == LogisticsPostalAddress.RecId`
- Delivery postal address details

### 9. CustTable
- **Join Type**: Outer Join
- **Join Condition**: `SalesTable.CustAccount == CustTable.AccountNum`
- Customer details

### 10. DlvTerm
- **Join Type**: Outer Join
- **Join Condition**: `SalesTable.DlvTerm == DlvTerm.Code`
- Delivery terms text

### 11. TSIUDI
- **Join Type**: Outer Join
- **Join Condition**: `ProdTable.ProdId == TSIUDI.ReferenceNumber AND ProdTable.ItemId == TSIUDI.ItemId AND InventDim.ConfigId == TSIUDI.ConfigId`
- **Filter**: `ReferenceType == ProductionOrder`
- UDI (Unique Device Identifier) lookup for production orders
- **Note**: Multiple UDI records can exist per production order (one per unit). The entity will return **multiple rows per production order** when multiple units exist. Filter by `UDIUnit` on the client side to get a specific unit.
- **Join Type**: Left Outer Join
- **Join Condition**: Complex barcode lookup logic (see Barcode Logic section)
- EAN codes

### 13. TSIUDI
- **Join Type**: Left Outer Join
- **Join Condition**: `ProdTable.ProdId == TSIUDI.ReferenceNumber AND TSIUDI.ReferenceType == TSIReferenceType::ProductionOrder AND ProdTable.ItemId == TSIUDI.ItemId AND InventDim.ConfigId == TSIUDI.ConfigId`
- UDI (Unique Device Identifier) lookup for production orders
- **Note**: Multiple UDI records can exist per production order (one per unit). The entity will return **multiple rows per production order** when multiple units exist. Filter by `UDIUnit` on the client side to get a specific unit.

## Field Mappings

| Field Name | Data Type | Source Table | Source Field | Mandatory | Description |
|-----------|-----------|--------------|--------------|-----------|-------------|
| `ProdId` | String (20) | `ProdTable` | `ProdId` | Yes | Production order ID (for filtering) |
| `LabelAddress` | String | `LogisticsPostalAddress` | `Address` | No | Delivery address (formatted) |
| `LabelAddressCountryRegionId` | String (10) | `CustTable` | `PartyCountry` | No | Customer country |
| `LabelSalesAddressCountryRegionId` | String (10) | `LogisticsPostalAddress` | `CountryRegionId` | No | Delivery country |
| `LabelConfigId` | String (10) | `InventDim` | `ConfigId` | No | Configuration |
| `LabelDateWeek` | String | Computed | `labelDateWeek()` | No | Date and week formatted (yyyy-mm-dd (week)) |
| `LabelDeliveryName` | String (60) | `SalesLine` | `DeliveryName` | No | Delivery name |
| `LabelDlvTermTempur` | String | `DlvTerm` | `Txt` | No | Delivery term text |
| `LabelItemFreeTxt` | String (60) | `SalesLine` | `Name` | No | Free text |
| `LabelItemId` | String (20) | `ProdTable` | `ItemId` | Yes | Item ID |
| `LabelProdId` | String (20) | `SalesLine` | `InventRefId` | No | Production ID reference |
| `LabelPurchOrderFormNum` | String (20) | `SalesTable` | `PurchOrderFormNum` | No | PO reference |
| `LabelSalesOrder` | String (20) | `SalesTable` | `SalesId` | No | Sales order number |
| `LabelEAN_Code` | String (80) | Computed | `labelEAN_Code()` | No | EAN barcode (complex lookup logic) |
| `LabelMadeIn` | String | Computed | `labelMadeIn()` | No | Made in country text |
| `LabelExternalItemId` | String (20) | `SalesLine` | `ExternalItemId` | No | External item reference |
| `UDI` | String | `TSIUDI` | `UDI` | No | Unique device ID (from custom TSIUDI table) |
| `UDIUnit` | String | `TSIUDI` | `Unit` | No | Unit for the UDI (e.g., 'x1', 'x10', 'x100') |
| `UDIRefRecId` | Int64 | `TSIUDI` | `RecId` | No | UDI record reference ID |
| `HasUDI` | Integer | Computed | `hasUDI()` | No | Indicates if UDI exists (1=yes, 0=no) |
| `dataAreaId` | String (4) | `ProdTable` | `dataAreaId` | Yes | Company identifier |

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

**Recommended Pattern for MES (Label + Logos in One Call):**
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'&$expand=Logos
```

**Note**: D365 Finance & Operations only supports **first-level $expand**. Query this entity directly with `$expand=Logos` to get label data and logos together.

### Required Query Parameters

**IMPORTANT**: This entity SHOULD be called with `ProdId` filter for optimal performance.

**Valid Query**:
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'
```

## Barcode Logic (LabelEAN_Code)

The EAN code field is computed using the `labelEAN_Code()` method with complex lookup logic:

```sql
CAST(
  (
    CASE
      WHEN EXISTS (
        SELECT 1
        FROM InventItemBarcode b
        INNER JOIN InventDim d ON d.InventDimId = b.InventDimId
        WHERE b.ItemId = @ItemId
          AND d.ConfigId = @ConfigId
      )
      THEN (
        SELECT TOP 1 b2.ItemBarcode
        FROM InventItemBarcode b2
        INNER JOIN InventDim d2 ON d2.InventDimId = b2.InventDimId
        WHERE b2.ItemId = @ItemId
          AND d2.ConfigId = @ConfigId
      )
      ELSE (
        SELECT TOP 1 b3.ItemBarcode
        FROM InventItemBarcode b3
        INNER JOIN InventDim d3 ON d3.InventDimId = b3.InventDimId
        WHERE b3.ItemId = @ItemId
          AND d3.ConfigId = 'Std'
      )
    END
  ) AS NVARCHAR(80)
) AS LabelEAN_Code
```

**Implementation Notes:**
- First check if barcode exists for the specific ConfigId from InventDim
- If found, use that barcode
- If not found, fallback to barcode with ConfigId = 'Std'
- This is implemented as a computed field method in X++

## MadeIn Logic (LabelMadeIn)

The MadeIn field is computed using the `labelMadeIn()` method:

```sql
('Made in ' + COALESCE(CountryName, '') +
 ' by/for Dan-Foam ApS, DK – a subsidiary of Tempur Sealy International, Inc.')
```

**Implementation Notes:**
- Source: `LogisticsAddressCountryRegionTranslation.ShortName` (translated country name)
- Fixed text: "Made in [country] by/for Dan-Foam ApS, DK – a subsidiary of Tempur Sealy International, Inc."
- If country name is null, returns just the fixed text
- This is implemented as a computed field method in X++

## HasUDI Logic (HasUDI)

The HasUDI field indicates whether a UDI record exists for the production order:

```sql
CASE WHEN TSIUDI.RecId IS NULL THEN 0 ELSE 1 END AS HasUDI
```

**Implementation Notes:**
- Returns 1 if `TSIUDI.RecId` exists, otherwise 0
- Used to quickly determine if UDI data is available
- This is implemented as a computed field method in X++

## Custom Fields Verification Required

The following fields are custom extensions and must be verified to exist in your D365 environment:

### TSIUDI Custom Table
- Custom table for storing UDI numbers per production/purchase order
- Fields: `ItemId`, `ReferenceType` (enum), `ReferenceNumber`, `ConfigId`, `Unit`, `UDI`
- Join on: `ProdId`, `ItemId`, `ConfigId`, and `ReferenceType = ProductionOrder`

### Logo Setup
- Logos are retrieved via the related `TSI_LabelLogoEntity` (navigation property)
- See [TSI_LabelLogoEntity.md](TSI_LabelLogoEntity.md) for logo setup tables and filtering logic

## Entity Properties

- **Public**: Yes
- **Allow Edit**: No (Read-only entity)
- **Data Management Enabled**: Yes
- **Data Management Staging Table**: TSI_LabelStaging
- **OData Enabled**: Yes
- **Primary Key**: EntityKey (ProdId, UDIRefRecId)
- **Primary Company Context**: DataAreaId

## Security

### Privileges Required
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

**Note**: Since the entity returns multiple rows per production order (one per unit), you should filter by `UDIUnit` to get the specific unit you need for label printing.

## TypeScript Interface

```typescript
export interface TSI_Label {
  ProdId: string; // Production order ID
  LabelAddress?: string;
  LabelAddressCountryRegionId?: string;
  LabelSalesAddressCountryRegionId?: string;
  LabelConfigId?: string;
  LabelDateWeek?: string; // Computed: "yyyy-mm-dd (week)" format
  LabelDeliveryName?: string;
  LabelDlvTermTempur?: string;
  LabelItemFreeTxt?: string;
  LabelItemId: string;
  LabelProdId?: string;
  LabelPurchOrderFormNum?: string;
  LabelSalesOrder?: string;
  LabelEAN_Code?: string;
  LabelMadeIn?: string;
  LabelExternalItemId?: string;
  UDI?: string;
  UDIUnit?: string;
  UDIRefRecId?: number;
  HasUDI?: number;
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
- [ ] Add all data sources with proper join types (OUTER JOIN for sales-related tables)
- [ ] Configure join conditions and ranges (InventRefType = Production, ReferenceType = ProductionOrder)
- [ ] Implement computed field methods: `labelDateWeek()`, `labelEAN_Code()`, `labelMadeIn()`, `hasUDI()`
- [ ] Create navigation property relationship to TSI_LabelLogoEntity (Logos)
- [ ] Map all fields from source tables
- [ ] Set entity properties (Public, OData enabled, Data Management staging table, etc.)
- [ ] Set primary key to EntityKey (ProdId, UDIRefRecId)
- [ ] Build and synchronize
- [ ] Grant security privileges
- [ ] Test OData endpoint
- [ ] Test with production orders that have sales orders
- [ ] Test with production orders that do NOT have sales orders
- [ ] Performance test with production data volume

## Testing Queries

### Verify Production Orders with Sales Orders
```sql
SELECT
  pt.ProdId,
  lpa.Address AS DeliveryAddress,
  lpa.CountryRegionId AS DeliveryCountryRegionId,
  ct.PartyCountry,
  sl.DeliveryName,
  st.SalesId,
  udi.UDI,
  udi.Unit AS UDIUnit
FROM ProdTable pt
INNER JOIN InventDim id ON pt.InventDimId = id.InventDimId
LEFT JOIN SalesLine sl ON pt.ProdId = sl.InventRefId AND sl.InventRefType = 1  -- Production
LEFT JOIN SalesTable st ON sl.SalesId = st.SalesId
LEFT JOIN LogisticsPostalAddress lpa ON st.DeliveryPostalAddress = lpa.RecId
LEFT JOIN CustTable ct ON st.CustAccount = ct.AccountNum
LEFT JOIN TSIUDI udi ON pt.ProdId = udi.ReferenceNumber
  AND udi.ReferenceType = 0  -- ProductionOrder
  AND pt.ItemId = udi.ItemId
  AND id.ConfigId = udi.ConfigId
WHERE pt.DataAreaId = '500'
  AND st.SalesId IS NOT NULL
```

### Verify Barcode Logic
```sql
-- Test barcode lookup for specific config
SELECT
  ItemId,
  ConfigId,
  ItemBarCode
FROM InventItemBarcode bib
INNER JOIN InventDim id ON bib.InventDimId = id.InventDimId
WHERE bib.ItemId = 'TEST-ITEM'
  AND (id.ConfigId = 'SpecificConfig' OR id.ConfigId = 'Std')
ORDER BY
  CASE WHEN id.ConfigId = 'SpecificConfig' THEN 1 ELSE 2 END
```

## Notes

- This is a read-only entity for querying label data for production orders
- Many fields are optional (Outer Joins) as not all production orders have sales orders
- Entity is designed for label printing scenarios only
- **MES Integration**: Prefer calling TSI_JobEntity with `$expand=Label($expand=Logos)` for single-call retrieval
- Can be queried directly with ProdId filter (required)
- Computed fields use SQL expressions for complex logic (barcode lookup, date formatting, country text)
- Logo files are retrieved via `$expand=Logos` navigation property (see TSI_LabelLogoEntity)
- Performance: Only query when printing labels, not for every production order check

### UDI (Unique Device Identifier) Logic
- UDI numbers are stored in the custom `TSIUDI` table
- Records are created per production order using `TSIUDI::createFromProdTable()`
- Join conditions: Match on `ProdId` (as ReferenceNumber), `ItemId`, `ConfigId`, and `ReferenceType = ProductionOrder`
- **Multiple rows per production order**: Entity returns one row per unit when multiple UDI records exist
- **Unit filtering**: MES should filter by `UDIUnit` in the OData query to get the desired unit (e.g., `$filter=UDIUnit eq 'x10'`)
- **Single call support**: All label data and UDI values available in one query, filtered by unit on client side
- UDI is generated using `TSIUDIHelper::generateUDI(ProdId, GTIN)`
- The TSIUDI table supports both production orders and purchase orders via the ReferenceType enum
- UDI records are created with item configuration and unit information from GTIN setup
