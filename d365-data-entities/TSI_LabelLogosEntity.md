# TSI_LabelLogosEntity - Production Label Logo Selection Entity Definition

## Overview

**Entity Name**: `TSI_LabelLogosEntity`
**Purpose**: Returns applicable logo files for label printing based on product family, destination country, and item attributes
**Primary Use**: MES system logo selection with complex filtering rules
**Relationship**: Child entity of TSI_LabelEntity (1:many relationship)

## Data Sources

### 1. TSI_LabelLogoHelperView (Primary Data Source)
- **Join Type**: Primary
- Helper view that encapsulates all logo filtering logic
- **Ranges Applied**:
  - `AttributeFilterOk = 1` (item attribute filters passed)
  - `CountryFilterOk = 1` (country filters passed)
  - `FamilyFilterOk = 1` (family filters passed)

## Field Mappings

| Field Name | Data Type | Source Table | Source Field | Mandatory | Description |
|-----------|-----------|--------------|--------------|-----------|-------------|
| `ProdId` | String (20) | `TSI_LabelLogoHelperView` | `ProdId` | Yes | Production order ID (for filtering) |
| `TSILogoId` | String | `TSI_LabelLogoHelperView` | `TSILogoId` | Yes | Logo identifier |
| `TSILogoPath` | String | `TSI_LabelLogoHelperView` | `TSILogoPath` | Yes | File path to logo image |
| `TSILogoPosition` | Integer | `TSI_LabelLogoHelperView` | `TSILogoPosition` | Yes | Display position on label (1-10) |
| `TSILogIdDescr` | String | `TSI_LabelLogoHelperView` | `TSILogIdDescr` | No | Logo description |

## Required Query Parameters

**IMPORTANT**: This entity SHOULD be called with `ProdId` filter for optimal performance.

**Note**: When accessed via `$expand=Logos` from TSI_LabelEntity, filtering is automatically handled by the parent entity.

**Valid Queries**:
```
GET /data/TSI_LabelLogos?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'
GET /data/TSI_Labels?$filter=ProdId eq 'PROD-001234'&$expand=Logos  // Automatic
```

## Logo Filtering Logic

The complex logo filtering logic (family, country, and item attribute filters) is now encapsulated within the `TSI_LabelLogoHelperView`. The entity applies three range filters to ensure only applicable logos are returned:

- **AttributeFilterOk = 1**: Item attribute filters have been validated
- **CountryFilterOk = 1**: Country/region filters have been validated
- **FamilyFilterOk = 1**: Product family filters have been validated

The helper view implements the same filtering logic as previously described, but consolidates it into a single view for better performance and maintainability.

### Result Ordering

Results are ordered by `TSILogoPosition` within the helper view to ensure consistent display order on labels.

## Entity Properties

- **Public**: Yes
- **Allow Edit**: No (Read-only entity)
- **Data Management Enabled**: Yes
- **Data Management Staging Table**: TSI_LabelLogosStaging
- **OData Enabled**: Yes
- **Primary Key**: EntityKey (TSILogoId, ProdId)
- **Public Collection Name**: TSI_LabelLogos
- **Public Entity Name**: TSI_LabelLogo

## Navigation Properties

### Relationship to TSI_LabelEntity
- **Type**: Many-to-One (Composition)
- **Foreign Key**: ProdId
- **Navigation Name**: `Label` (from logo to parent label)
- **Inverse Navigation**: `Logos` (from label to logos)
- **Relationship Type**: Composition

**Navigation Chain (First-Level Expansion Only):**
- TSI_LabelEntity → `Logos` → TSI_LabelLogosEntity (first level only)
- **D365 Limitation**: Only first-level $expand supported - nested expansion NOT supported
- **Recommended**: Query TSI_LabelEntity directly: `GET /data/TSI_Labels?$filter=ProdId eq 'PROD-001234'&$expand=Logos`

## Security

### Privileges Required
- Read access to `TSI_LabelLogoHelperView`
- Execute permission on `TSI_LabelLogosEntity`

## Performance Considerations

### Recommended Indexes
```sql
-- On TSI_LabelLogoHelperView for ProdId filtering
CREATE INDEX IX_TSI_LabelLogoHelperView_ProdId
  ON TSI_LabelLogoHelperView(ProdId)
  INCLUDE (TSILogoId, TSILogoPosition);

-- Ensure underlying TSILogoSetup tables are properly indexed
-- (See TSILogoSetup table indexes in the helper view implementation)
```

### Caching Strategy
- Logo setup tables (TSILogoSetup, TSILogoSetupFamily, TSILogoSetupCounty) are cached in the helper view
- Helper view results may be cached depending on implementation
- Logo configurations rarely change

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
      "ProdId": "PROD-001234",
      "TSILogoId": "CEMD",
      "TSILogoPath": "\\\\server\\logos\\cemd.png",
      "TSILogoPosition": 1,
      "TSILogIdDescr": "CE Medical Device"
    },
    {
      "ProdId": "PROD-001234",
      "TSILogoId": "Triman",
      "TSILogoPath": "\\\\server\\logos\\triman.png",
      "TSILogoPosition": 2,
      "TSILogIdDescr": "French Recycling Logo"
    }
  ]
}
```

### Example - Expand Logos from Label Entity
```
GET /data/TSI_Labels?$filter=dataAreaId eq '500' and ProdId eq 'PROD-001234'&$expand=Logos
```

### Example Expanded Response
```json
{
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
  ProdId: string; // Production order ID
  TSILogoId: string;
  TSILogoPath: string;
  TSILogoPosition: number;
  TSILogIdDescr?: string;
}

// Extended Label interface with logos
export interface TSI_LabelWithLogos extends TSI_Label {
  Logos?: TSI_LabelLogo[];
}
```

## Implementation Checklist

- [ ] Verify `TSI_LabelLogoHelperView` exists and implements all filtering logic
- [ ] Create entity in D365
- [ ] Add TSI_LabelLogoHelperView as primary data source
- [ ] Configure ranges: AttributeFilterOk=1, CountryFilterOk=1, FamilyFilterOk=1
- [ ] Map the 5 fields from the helper view
- [ ] Set entity properties (Public, OData enabled, Data Management staging table, etc.)
- [ ] Set primary key to EntityKey (TSILogoId, ProdId)
- [ ] Create navigation property relationship to TSI_LabelEntity
- [ ] Build and synchronize
- [ ] Grant security privileges
- [ ] Test OData endpoint standalone
- [ ] Test $expand from TSI_LabelEntity
- [ ] Test with production orders having different family/country/attribute combinations
- [ ] Verify multiple logos return correctly
- [ ] Performance test with production data volume

## Testing Queries

### Verify Logo Setup Data
```sql
-- Check the helper view directly for a specific production order
SELECT * FROM TSI_LabelLogoHelperView
WHERE ProdId = 'PROD-001234'
  AND AttributeFilterOk = 1
  AND CountryFilterOk = 1
  AND FamilyFilterOk = 1
ORDER BY TSILogoPosition;
```

### Verify Logo Filtering for Production Order
```sql
-- Test the entity directly
SELECT ProdId, TSILogoId, TSILogoPath, TSILogoPosition, TSILogIdDescr
FROM TSI_LabelLogosEntity
WHERE ProdId = 'PROD-001234'
ORDER BY TSILogoPosition;
```

## Notes

- This entity returns **multiple rows per production order** (one per applicable logo)
- Complex filtering logic is encapsulated in the `TSI_LabelLogoHelperView`
- Logos are filtered based on product family, destination country, and item attributes within the helper view
- **MES Integration**: D365 only supports first-level $expand. Query TSI_LabelEntity directly with `$expand=Logos` to get labels and logos together
- **Important**: Nested expansion like `$expand=Label($expand=Logos)` is NOT supported in D365 Finance & Operations
- Logo positions (1-10) determine display order on the label
- If no logos match the filters, no rows are returned for that production order
- The helper view handles all the complex filtering logic for better performance and maintainability

## Custom Tables and Views Required

### TSI_LabelLogoHelperView
- **Purpose**: Encapsulates all logo filtering logic for better performance and maintainability
- **Key Fields**:
  - `ProdId` (Production order ID)
  - `TSILogoId` (Logo identifier)
  - `TSILogoPath` (File path)
  - `TSILogoPosition` (Display position)
  - `TSILogIdDescr` (Description)
  - `AttributeFilterOk` (Boolean: attribute filters passed)
  - `CountryFilterOk` (Boolean: country filters passed)
  - `FamilyFilterOk` (Boolean: family filters passed)

### Underlying Tables (used by helper view)
- **TSILogoSetup**: Main logo configuration
- **TSILogoSetupFamily**: Family dimension filters
- **TSILogoSetupCounty**: Country/region filters
- Plus related tables for product attributes, sales orders, etc.

## Additional Considerations

### Helper View Implementation
The `TSI_LabelLogoHelperView` encapsulates all the complex filtering logic that was previously implemented directly in the entity. This provides:

- **Better Performance**: Pre-filtered results reduce query complexity
- **Maintainability**: Filtering logic centralized in one view
- **Reusability**: Helper view can be used by other entities or reports
- **Testing**: Easier to test filtering logic independently

### Item Attribute Implementation
The helper view handles item attribute filtering where logos require specific boolean product attributes to be set to Yes. This logic is now implemented within the view for optimal performance.

### Packed Query Field
The underlying `TSILogoSetup.Query` field may still contain packed query logic for additional filtering rules. This is handled within the helper view implementation.
