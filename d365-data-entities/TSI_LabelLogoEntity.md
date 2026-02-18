# TSI_LabelLogoEntity - Job Label Logo Selection Entity Definition

## Overview

**Entity Name**: `TSI_LabelLogoEntity`
**Purpose**: Returns applicable logo files for label printing based on product family, destination country, and item attributes
**Primary Use**: MES system logo selection with complex filtering rules
**Relationship**: Child entity of TSI_LabelEntity (1:many relationship)

## Data Sources

### 1. ProdTable (Primary Data Source)
- **Join Type**: Primary
- Production order header information

### 2. InventTable
- **Join Type**: Inner Join
- **Join Condition**: `JmgJobTable.ItemId == InventTable.ItemId`
- Item master data

### 3. SalesLine
- **Join Type**: Left Outer Join
- **Join Condition**: `ProdTable.ProdId == SalesLine.InventRefId`
- Sales order line reference

### 4. SalesTable
- **Join Type**: Left Outer Join
- **Join Condition**: `SalesLine.SalesId == SalesTable.SalesId`
- Customer delivery information

### 5. LogisticsPostalAddress
- **Join Type**: Left Outer Join
- **Join Condition**: `SalesTable.DeliveryPostalAddress == LogisticsPostalAddress.RecId`
- Delivery postal address details (for country filtering)

### 6. TSILogoSetup
- **Join Type**: Inner Join
- **Join Condition**: Complex filtering (see Logo Filtering Logic section)
- Main logo configuration table

### 7. TSILogoSetupFamily
- **Join Type**: Left Outer Join
- **Join Condition**: `TSILogoSetup.TSILogoId == TSILogoSetupFamily.TSILogoId`
- Family filter records (optional)

### 8. TSILogoSetupCounty
- **Join Type**: Left Outer Join
- **Join Condition**: `TSILogoSetup.TSILogoId == TSILogoSetupCounty.TSILogoId`
- Country filter records (optional)

### 9. EcoResProduct
- **Join Type**: Left Outer Join
- **Join Condition**: `InventTable.Product == EcoResProduct.RecId`
- Product master record (for attribute lookups)

### 10. EcoResAttribute
- **Join Type**: Left Outer Join
- **Join Condition**: `EcoResAttribute.Name == TSILogoSetup.TSILogoId`
- Attribute definitions (to check if logo ID matches an attribute name)

### 14. EcoResProductAttributeValue
- **Join Type**: Left Outer Join
- **Join Condition**: `EcoResProduct.RecId == EcoResProductAttributeValue.Product AND EcoResAttribute.RecId == EcoResProductAttributeValue.Attribute`
- Product attribute values (to check if boolean attribute is true)

## Field Mappings

| Field Name | Data Type | Source Table | Source Field | Mandatory | Description |
|-----------|-----------|--------------|--------------|-----------|-------------|
| `RecId` | Int64 | `ProdTable` | `RecId` | Yes | Primary key (links to TSI_LabelEntity) |
| `ProdId` | String (20) | `ProdTable` | `ProdId` | Yes | Production order ID (for filtering) |
| `TSILogoId` | String | `TSILogoSetup` | `TSILogoId` | Yes | Logo identifier |
| `TSILogoPath` | String | `TSILogoSetup` | `TSILogoPath` | Yes | File path to logo image |
| `TSILogoPosition` | Integer | `TSILogoSetup` | `TSILogoPosition` | Yes | Display position on label (1-10) |
| `TSILogIdDescr` | String | `TSILogoSetup` | `TSILogIdDescr` | No | Logo description |
| `dataAreaId` | String (4) | `ProdTable` | `dataAreaId` | Yes | Company identifier |

## Required Query Parameters

**IMPORTANT**: This entity MUST be called with `ProdId` filter to prevent full table scans.

**Note**: When accessed via `$expand=Logos` from TSI_LabelEntity, this validation is automatically satisfied by the parent entity's filters.

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
            error("TSI_LabelLogoEntity must be filtered by JobId or ProdId.");
            ret = false;
        }
    }

    return ret;
}
```

**Valid Queries**:
```
GET /data/TSI_LabelLogos?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'
GET /data/TSI_Labels?$filter=ProdId eq 'PROD-001234'&$expand=Logos  // Automatic
```

## Logo Filtering Logic

The entity implements complex filtering to determine which logos apply to each job. Multiple logos can apply, resulting in multiple rows per job.

### Filter 1: Active Logos Only

```xpp
WHERE TSILogoSetup.TSILogoActive == NoYes::Yes
```

Only logos marked as active are considered.

### Filter 2: Family Filter

```xpp
AND (
    // Logo applies to all families
    TSILogoSetup.TSIAllFamilies == NoYes::Yes

    OR

    // Logo applies only to specific families
    (
        TSILogoSetup.TSIAllFamilies == NoYes::No
        AND EXISTS (
            SELECT 1 FROM TSILogoSetupFamily
            WHERE TSILogoSetupFamily.TSILogoId == TSILogoSetup.TSILogoId
              AND TSILogoSetupFamily.Value == [ProductFamilyDimension]
        )
    )
)
```

**Implementation Notes:**
- Product family dimension value must be retrieved from the item's financial dimensions
- Dimension name: `F_Family`
- If `TSIAllFamilies = Yes`, logo applies regardless of family
- If `TSIAllFamilies = No`, logo only applies if product's family exists in TSILogoSetupFamily

### Filter 3: Country Filter

```xpp
AND (
    // No country filter exists (applies to all countries)
    NOT EXISTS (
        SELECT 1 FROM TSILogoSetupCounty
        WHERE TSILogoSetupCounty.TSILogoId == TSILogoSetup.TSILogoId
    )

    OR

    // Country filter exists and matches destination country
    EXISTS (
        SELECT 1 FROM TSILogoSetupCounty
        WHERE TSILogoSetupCounty.TSILogoId == TSILogoSetup.TSILogoId
          AND TSILogoSetupCounty.CountryRegionId == LogisticsPostalAddress.CountryRegionId
    )
)
```

**Implementation Notes:**
- If no TSILogoSetupCounty records exist for a logo, it applies to all countries
- If TSILogoSetupCounty records exist, the sales order destination country must match
- Destination country comes from `LogisticsPostalAddress.CountryRegionId` via SalesTable

### Filter 4: Item Attribute Filter (Specific Logos Only)

Some logos require specific boolean **product attributes** to be set to Yes. This filter **only affects logos whose ID matches a product attribute name**.

**Examples:**
- Logo ID `UDI` requires product attribute `UDI` = NoYes::Yes
- Logo ID `CEMD` requires product attribute `CE-Mark` = NoYes::Yes
- Logo ID `Triman` does NOT match any attribute → no attribute check (logo applies normally)

**Implementation Approach:**
```xpp
AND (
    // Logo ID doesn't match any attribute name (no attribute check needed)
    // This applies to most logos like Triman, Japan Recycle, etc.
    NOT EXISTS (
        SELECT 1 FROM EcoResAttribute
        WHERE EcoResAttribute.Name == TSILogoSetup.TSILogoId
    )

    OR

    // Logo ID matches attribute name (e.g., UDI, CEMD)
    // AND attribute value is true for this product
    EXISTS (
        SELECT 1
        FROM EcoResAttribute
        INNER JOIN EcoResProductAttributeValue ON EcoResAttribute.RecId == EcoResProductAttributeValue.Attribute
        WHERE EcoResAttribute.Name == TSILogoSetup.TSILogoId
          AND EcoResProductAttributeValue.Product == EcoResProduct.RecId
          AND EcoResProductAttributeValue.Value == 1  // NoYes::Yes (boolean product attribute)
    )
)
```

**Implementation Notes:**
- Check if `TSILogoSetup.TSILogoId` matches a product attribute name in `EcoResAttribute.Name`
- If no match, logo applies (no attribute restriction) - **this is the case for most logos**
- If match exists (e.g., logo ID is "UDI" or "CEMD"), verify the product attribute value is NoYes::Yes (1) for the product
- Only logos with IDs matching product attribute names (like UDI, CEMD) are subject to this filter
- Boolean product attributes store 1 for NoYes::Yes, 0 for NoYes::No
- Requires joins to EcoResProduct (data source #12), EcoResAttribute (#13), and EcoResProductAttributeValue (#14)

### Result Ordering

```xpp
ORDER BY TSILogoSetup.TSILogoPosition, TSILogoSetup.TSILogoId
```

## Entity Properties

- **Public**: Yes
- **Allow Edit**: No (Read-only entity)
- **Data Management Enabled**: Yes
- **OData Enabled**: Yes
- **Primary Key**: RecId, TSILogoId, dataAreaId
- **Primary Index**: Composite key on RecId + TSILogoId

## Navigation Properties

### Relationship to TSI_LabelEntity
- **Type**: Many-to-One
- **Foreign Key**: RecId
- **Navigation Name**: `Label` (from logo to parent label)
- **Inverse Navigation**: `Logos` (from label to logos)

**Navigation Chain (First-Level Expansion Only):**
- TSI_LabelEntity → `Logos` → TSI_LabelLogoEntity (first level only)
- **D365 Limitation**: Only first-level $expand supported - nested expansion NOT supported
- **Recommended**: Query TSI_LabelEntity directly: `GET /data/TSI_Labels?$filter=ProdId eq 'PROD-001234'&$expand=Logos`

## Security

### Privileges Required
- Read access to `ProdTable`
- Read access to `InventTable`
- Read access to `SalesLine`
- Read access to `SalesTable`
- Read access to `LogisticsPostalAddress`
- Read access to `TSILogoSetup`
- Read access to `TSILogoSetupFamily`
- Read access to `TSILogoSetupCounty`
- Read access to `EcoResProduct`
- Read access to `EcoResProductAttributeValue`
- Execute permission on `TSI_LabelLogoEntity`
- View permission for Sales and Job Management modules

## Performance Considerations

### Recommended Indexes
```sql
-- On TSILogoSetup for active logo lookup
CREATE INDEX IX_TSILogoSetup_Active
  ON TSILogoSetup(TSILogoActive, TSILogoPosition)
  INCLUDE (TSILogoId, TSILogoPath);

-- On TSILogoSetupFamily for family filtering
CREATE INDEX IX_TSILogoSetupFamily_Value
  ON TSILogoSetupFamily(Value, TSILogoId);

-- On TSILogoSetupCounty for country filtering
CREATE INDEX IX_TSILogoSetupCounty_Country
  ON TSILogoSetupCounty(CountryRegionId, TSILogoId);
```

### Caching Strategy
- Logo setup tables (TSILogoSetup, TSILogoSetupFamily, TSILogoSetupCounty) are set to `CacheLookup = EntireTable`
- Logo configurations rarely change
- Cache invalidation on logo setup modifications

## OData Endpoint

```
GET /data/TSI_LabelLogos
```

### Example Query - Get Logos for Specific Production Order
```
GET /data/TSI_LabelLogos?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'&$orderby=TSILogoPosition
```

### Example Response
```json
{
  "value": [
    {
      "RecId": 5637144576,
      "ProdId": "PROD-001234",
      "TSILogoId": "CEMD",
      "TSILogoPath": "\\\\server\\logos\\cemd.png",
      "TSILogoPosition": 1,
      "TSILogIdDescr": "CE Medical Device",
      "dataAreaId": "500"
    },
    {
      "RecId": 5637144576,
      "ProdId": "PROD-001234",
      "TSILogoId": "Triman",
      "TSILogoPath": "\\\\server\\logos\\triman.png",
      "TSILogoPosition": 2,
      "TSILogIdDescr": "French Recycling Logo",
      "dataAreaId": "500"
    }
  ]
}
```

### Example - Expand Logos from Label Entity
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'&$expand=Logos($orderby=TSILogoPosition)
```

### Example Expanded Response
```json
{
  "RecId": 5637144576,
  "ProdId": "PROD-001234",
  "LabelAddress": "123 Main St",
  "LabelSalesOrder": "SO-001234",
  "UDI": "(01)12345678901234(21)PROD-001",
  "UDIUnit": "x1",
  "dataAreaId": "500",
  "Logos": [
    {
      "TSILogoId": "CEMD",
      "TSILogoPath": "\\\\server\\logos\\cemd.png",
      "TSILogoPosition": 1,
      "TSILogIdDescr": "CE Medical Device"
    },
    {
      "TSILogoId": "Triman",
      "TSILogoPath": "\\\\server\\logos\\triman.png",
      "TSILogoPosition": 2,
      "TSILogIdDescr": "French Recycling Logo"
    }
  ]
}
```

## TypeScript Interface

```typescript
export interface TSI_LabelLogo {
  RecId: number; // Links to TSI_Label.RecId (ProdTable.RecId)
  ProdId: string; // Production order ID
  TSILogoId: string;
  TSILogoPath: string;
  TSILogoPosition: number;
  TSILogIdDescr?: string;
  dataAreaId: string;
}

// Extended Label interface with logos
export interface TSI_LabelWithLogos extends TSI_Label {
  Logos?: TSI_LabelLogo[];
}
```

## Implementation Checklist

- [ ] Verify all custom tables exist in D365 (TSILogoSetup, TSILogoSetupFamily, TSILogoSetupCounty)
- [ ] Create entity in D365
- [ ] Add all data sources with proper join types
- [ ] Implement Filter 1: Active logos only
- [ ] Implement Filter 2: Family filter logic (TSIAllFamilies flag + TSILogoSetupFamily lookup)
- [ ] Implement Filter 3: Country filter logic (EXISTS checks for TSILogoSetupCounty)
- [ ] Implement Filter 4: Item attribute filter logic (boolean attributes)
- [ ] Retrieve product family dimension (F_Family) from item
- [ ] Map all fields from source tables
- [ ] Implement validateRead() method to enforce ProdId filter
- [ ] Configure ordering by TSILogoPosition
- [ ] Set entity properties (Public, OData enabled, etc.)
- [ ] Create navigation property relationship to TSI_LabelEntity
- [ ] Build and synchronize
- [ ] Grant security privileges
- [ ] Test OData endpoint standalone
- [ ] Test $expand from TSI_LabelEntity
- [ ] Test with jobs having different family/country/attribute combinations
- [ ] Verify multiple logos return correctly
- [ ] Performance test with production data volume

## Testing Queries

### Verify Logo Setup Data
```sql
-- Check active logos
SELECT TSILogoId, TSILogIdDescr, TSILogoPath, TSILogoPosition, TSIAllFamilies, TSILogoActive
FROM TSILogoSetup
WHERE TSILogoActive = 1
ORDER BY TSILogoPosition;

-- Check family filters
SELECT ls.TSILogoId, lsf.Value, lsf.Description
FROM TSILogoSetup ls
LEFT JOIN TSILogoSetupFamily lsf ON ls.TSILogoId = lsf.TSILogoId
WHERE ls.TSILogoActive = 1;

-- Check country filters
SELECT ls.TSILogoId, lsc.CountryRegionId, lsc.ShortName
FROM TSILogoSetup ls
LEFT JOIN TSILogoSetupCounty lsc ON ls.TSILogoId = lsc.TSILogoId
WHERE ls.TSILogoActive = 1;
```

### Verify Logo Filtering for Job
```sql
-- Test complete filtering logic for a specific production order
SELECT
  pt.RecId,
  pt.ProdId,
  ls.TSILogoId,
  ls.TSILogoPath,
  ls.TSILogoPosition,
  ls.TSILogIdDescr,
  ls.TSIAllFamilies,
  lpa.CountryRegionId AS DestCountry
FROM ProdTable pt
LEFT JOIN SalesLine sl ON pt.ProdId = sl.InventRefId
LEFT JOIN SalesTable st ON sl.SalesId = st.SalesId
LEFT JOIN LogisticsPostalAddress lpa ON st.DeliveryPostalAddress = lpa.RecId
CROSS JOIN TSILogoSetup ls
WHERE pt.ProdId = 'PROD-001234' -- Specific production order
  AND ls.TSILogoActive = 1
  -- Add family and country filter logic here
ORDER BY ls.TSILogoPosition;
```

## Notes

- This entity returns **multiple rows per job** (one per applicable logo)
- Logos are filtered based on product family, destination country, and item attributes
- **MES Integration**: D365 only supports first-level $expand. Query TSI_LabelEntity directly with `$expand=Logos` to get labels and logos together
- **Important**: Nested expansion like `$expand=Label($expand=Logos)` is NOT supported in D365 Finance & Operations
- Logo positions (1-10) determine display order on the label
- If no logos match the filters, no rows are returned for that job
- Logo setup tables are cached for performance
- The packed query in `TSILogoSetup.Query` field may provide additional filtering rules (implementation TBD)

## Custom Tables Required

### TSILogoSetup
- **Purpose**: Main logo configuration
- **Key Fields**:
  - `TSILogoId` (PK)
  - `TSILogoPath` (file path)
  - `TSILogoActive` (NoYes enum)
  - `TSILogoPosition` (Integer 1-10)
  - `TSIAllFamilies` (NoYes enum)
  - `TSILateDedicationId`
  - `Query` (Container - packed query)

### TSILogoSetupFamily
- **Purpose**: Family dimension filters
- **Key Fields**:
  - `TSILogoId` (PK, FK)
  - `Value` (PK, dimension value)
  - `Description`

### TSILogoSetupCounty
- **Purpose**: Country/region filters
- **Key Fields**:
  - `TSILogoId` (PK, FK)
  - `CountryRegionId` (PK)
  - `ShortName`

## Additional Considerations

### Item Attribute Implementation
The entity needs to:
1. Identify if logo ID matches an item attribute name
2. Look up the attribute value for the current item
3. Check if the boolean value is true
4. Only include logo if attribute check passes

This may require:
- Additional data sources for `EcoResProduct` and `EcoResProductAttributeValue`
- Complex WHERE clause or computed field logic
- Performance consideration: attribute lookups can be expensive

### Packed Query Field
The `TSILogoSetup.Query` field contains a packed QueryRun that may provide additional filtering rules. Implementation details TBD - this field may need to be unpacked and executed against the current record to determine applicability.
