# TSI_ItemEntity - Item Master Entity Definition

## Overview

**Entity Name**: `TSI_ItemEntity`
**Purpose**: Item master data for MES operations
**Primary Use**: MES system reference data for items (basic properties, BOM units, dimensions)

## Data Sources

### 1. InventTable (Primary Data Source)
- **Join Type**: Primary
- Released products (legal entity specific)

### 2. EcoResProduct
- **Join Type**: Inner Join
- **Join Condition**: `InventTable.Product == EcoResProduct.RecId`
- Global product master

### 3. EcoResProductSystemLanguage
- **Join Type**: Outer Join
- **Join Condition**: `EcoResProduct.RecId == EcoResProductSystemLanguage.Product`
- System language for product translations

### 4. EcoResProductTranslation
- **Join Type**: Outer Join
- **Join Condition**:
  - `EcoResProduct.RecId == EcoResProductTranslation.Product`
  - `EcoResProductSystemLanguage.LanguageId == EcoResProductTranslation.SystemLanguageId`
- Product name translations (multi-language support)

### 5. InventItemGroupItem
- **Join Type**: Outer Join
- **Join Condition**: Uses standard relations (InventTable relation)
- Item-to-item-group assignment

## Field Mappings

| Field Name | Data Type | Source Table | Source Field | Mandatory | Description |
|-----------|-----------|--------------|--------------|-----------|-------------|
| `ItemId` | String (20) | `InventTable` | `ItemId` | No | Item identifier |
| `ItemName` | String (60) | `EcoResProductTranslation` | `Name` | No | Item name/description (translated) |
| `ItemGroupId` | String (10) | `InventItemGroupItem` | `ItemGroupId` | No | Item group identifier |
| `ABCValue` | Enum | `InventTable` | `ABCValue` | No | ABC classification |
| `BOMUnitId` | String (10) | `InventTable` | `BOMUnitId` | No | BOM unit of measure |

## Query Filters

### Range on Primary Data Source
```xpp
// No filter on primary data source - returns all released products per company
```

## Entity Properties

- **Public**: Yes
- **Allow Edit**: No (Read-only entity)
- **Data Management Enabled**: Yes
- **Data Management Staging Table**: TSI_ItemStaging
- **OData Enabled**: Yes
- **Is Read Only**: Yes
- **Primary Key**: EntityKey (ItemId)
- **Primary Company Context**: DataAreaId
- **Configuration Key**: LogisticsBasic
- **Form Reference**: EcoResProductInformation
- **Label**: @TSI:ItemsEntityMES
- **Public Collection Name**: TSI_Items
- **Public Entity Name**: TSI_Item

## Security

### Privileges Required
- Read access to `InventTable`
- Read access to `EcoResProduct`
- Read access to `EcoResProductTranslation`
- Read access to `InventItemGroupItem`
- Execute permission on `TSI_ItemEntity`
- View permission for Product Information Management module

## Performance Considerations

### Recommended Indexes
```sql
-- On InventTable
CREATE INDEX IX_InventTable_ItemId_DataArea
  ON InventTable(ItemId, DataAreaId);

-- On InventItemGroupItem
CREATE INDEX IX_InventItemGroupItem_ItemId
  ON InventItemGroupItem(ItemId, ItemDataAreaId);
```

## OData Endpoint

```
GET /data/TSI_Items
```

### Example Query - Get Specific Item
```
GET /data/TSI_Items?$filter=ItemId eq 'PILLOW-001'
```

### Example Query - Get Items by Group
```
GET /data/TSI_Items?$filter=ItemGroupId eq 'PILLOW'
```

### Example Query - Select Specific Fields
```
GET /data/TSI_Items?$select=ItemId,ItemName,ItemGroupId,BOMUnitId
```

## TypeScript Interface

```typescript
export interface TSI_Item {
  ItemId: string;
  ItemName: string;
  ItemGroupId: string;
  ABCValue: number; // 0=A, 1=B, 2=C
  BOMUnitId: string;
}

// Helper for ABC classification
export enum ABCClassification {
  A = 0,
  B = 1,
  C = 2
}
```

## Implementation Checklist

- [ ] Verify all data sources exist in D365
- [ ] Create entity in D365
- [ ] Add all data sources with proper join types
- [ ] Configure join conditions
- [ ] Map all fields from source tables
- [ ] Set entity properties (Public, OData enabled, etc.)
- [ ] Build and synchronize
- [ ] Grant security privileges
- [ ] Test OData endpoint
- [ ] Test multi-language support for ItemName
- [ ] Performance test with production data volume

## Testing Queries

### Verify Data in SQL
```sql
SELECT
    it.ITEMID,
    pt.NAME AS ITEMNAME,
    iig.ITEMGROUPID,
    it.ABCVALUE,
    it.BOMUNITID
FROM InventTable it
INNER JOIN EcoResProduct p ON it.PRODUCT = p.RECID
LEFT JOIN EcoResProductSystemLanguage psl ON p.RECID = psl.PRODUCT
LEFT JOIN EcoResProductTranslation pt
    ON p.RECID = pt.PRODUCT
    AND psl.LanguageId = pt.SystemLanguageId
LEFT JOIN InventItemGroupItem iig ON it.ITEMID = iig.ITEMID
ORDER BY it.ITEMID
```

## Notes

- This is a read-only entity for querying item master data
- Returns one row per item
- Item names support multi-language via EcoResProductTranslation
- ItemGroupId comes from InventItemGroupItem relation table
- BOMUnitId is directly on InventTable
- ABCValue is on InventTable
- Entity is optimized for fast, frequent access
- Good candidate for caching as item master data changes infrequently
