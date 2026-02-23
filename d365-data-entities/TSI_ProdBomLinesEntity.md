# TSI_ProdBOMLinesEntity - Production BOM Lines Entity Definition

## Overview

**Entity Name**: `TSI_ProdBOMLinesEntity`
**Purpose**: Production BOM lines with item details and inventory dimensions
**Primary Use**: MES system material requirements and consumption tracking

## Data Sources

### 1. ProdBOM (Primary Data Source)
- **Join Type**: Primary
- Production BOM lines

### 2. InventTable
- **Join Type**: Inner Join
- **Join Condition**: `ProdBOM.ItemId == InventTable.ItemId`
- Released products (company-specific)

### 3. EcoResProduct
- **Join Type**: Inner Join
- **Join Condition**: `InventTable.Product == EcoResProduct.RecId`
- Global product master

### 4. EcoResProductSystemLanguage
- **Join Type**: Outer Join
- **Join Condition**: `EcoResProduct.RecId == EcoResProductSystemLanguage.Product`
- System language for product translations

### 5. EcoResProductTranslation
- **Join Type**: Outer Join
- **Join Condition**:
  - `EcoResProduct.RecId == EcoResProductTranslation.Product`
  - `EcoResProductSystemLanguage.LanguageId == EcoResProductTranslation.SystemLanguageId`
- Product name translations (multi-language support)

### 6. InventItemGroupItem
- **Join Type**: Outer Join
- **Join Condition**:
  - `InventTable.ItemId == InventItemGroupItem.ItemId`
  - `InventTable.dataAreaId == InventItemGroupItem.ItemDataAreaId`
- Item-to-item-group assignment

### 7. TSIInventTable
- **Join Type**: Outer Join
- **Join Condition**: `InventTable.ItemId == TSIInventTable.TSIItemId`
- Custom InventTable extension for additional fields

### 8. InventDim
- **Join Type**: Inner Join
- **Join Condition**: `ProdBOM.InventDimId == InventDim.InventDimId`
- Inventory dimensions

## Field Mappings

| Field Name | Data Type | Source Table | Source Field | Mandatory | Description |
|-----------|-----------|--------------|--------------|-----------|-------------|
| `ProdId` | String (20) | `ProdBOM` | `ProdId` | Yes | Production order ID |
| `ItemId` | String (20) | `ProdBOM` | `ItemId` | Yes | Component item ID |
| `LineNum` | Real | `ProdBOM` | `LineNum` | No | BOM line number |
| `BOMQty` | Real | `ProdBOM` | `BOMQty` | No | Required quantity |
| `BOMQtySerie` | Real | `ProdBOM` | `BOMQtySerie` | No | Series quantity |
| `UnitId` | String (10) | `ProdBOM` | `UnitId` | No | Unit of measure |
| `Position` | Integer | `ProdBOM` | `Position` | No | Position in BOM |
| `ScrapVar` | Real | `ProdBOM` | `ScrapVar` | No | Variable scrap percentage |
| `ItemName` | String (60) | `EcoResProductTranslation` | `Name` | No | Item description (translated) |
| `NameAlias` | String (60) | `InventTable` | `NameAlias` | No | Item alias name |
| `ItemGroupId` | String (10) | `InventItemGroupItem` | `ItemGroupId` | No | Item group |
| `Depth` | Real | `InventTable` | `Depth` | No | Item depth/length |
| `Width` | Real | `InventTable` | `Width` | No | Item width |
| `Height` | Real | `InventTable` | `Height` | No | Item height |
| `TSIShorteningLength` | Real | `TSIInventTable` | `TSIShorteningLength` | No | Shortening length (custom field) |
| `TSIBlockWidth` | Real | `TSIInventTable` | `TSIBlockWidth` | No | Block width (custom field) |
| `InventLocationId` | String (10) | `InventDim` | `InventLocationId` | No | Warehouse |
| `wMSLocationId` | String (20) | `InventDim` | `wMSLocationId` | No | WMS location |
| `inventDimId` | String (20) | `ProdBOM` | `inventDimId` | No | Dimension ID |
| `InventTransId` | String (20) | `ProdBOM` | `InventTransId` | Yes | Transaction ID |
| `InventRefType` | Enum | `ProdBOM` | `InventRefType` | No | Reference type |
| `InventRefId` | String (20) | `ProdBOM` | `InventRefId` | No | Reference ID |

## Query Filters

### Range on Primary Data Source
```xpp
// No filter on primary data source - returns all BOM lines
```

## Custom Fields Verification Required

The following fields are custom extensions and must be verified to exist in your D365 environment:

### TSIInventTable Custom Fields
- `TSIShorteningLength`
- `TSIBlockWidth`

## Entity Properties

- **Public**: Yes
- **Allow Edit**: No (Read-only entity)
- **Data Management Enabled**: Yes
- **Data Management Staging Table**: TSI_ProdBOMLinesStaging
- **OData Enabled**: Yes
- **Is Read Only**: Yes
- **Primary Key**: EntityKey (InventTransId, LineNum, ProdId)
- **Primary Company Context**: DataAreaId
- **Configuration Key**: Prod
- **Label**: @TSI:ProductionBOMLines
- **Public Collection Name**: TSI_ProdBOMLines
- **Public Entity Name**: TSI_ProdBOMLine

## Security

### Privileges Required
- Read access to `ProdBOM`
- Read access to `InventTable`
- Read access to `EcoResProduct`
- Read access to `EcoResProductTranslation`
- Read access to `InventItemGroupItem`
- Read access to `TSIInventTable`
- Read access to `InventDim`
- Execute permission on `TSI_ProdBomLinesEntity`
- View permission for Production Control module

## Performance Considerations

### Recommended Indexes
```sql
-- On ProdBOM
CREATE INDEX IX_ProdBOM_ProdId_LineNum
  ON ProdBOM(ProdId, LineNum, DataAreaId)
  INCLUDE (ItemId, BOMQty, UnitId, InventDimId);

-- On ProdBOM for ItemId lookups
CREATE INDEX IX_ProdBOM_ItemId
  ON ProdBOM(ItemId, DataAreaId)
  INCLUDE (ProdId, LineNum);

-- On InventDim
CREATE INDEX IX_InventDim_InventDimId
  ON InventDim(InventDimId)
  INCLUDE (InventLocationId, WMSLocationId);
```

## OData Endpoint

```
GET /data/TSI_ProdBOMLines
```

### Example Query - Get BOM Lines for Production Order
```
GET /data/TSI_ProdBOMLines?$filter=ProdId eq '000123'
```

### Example Query - Get BOM Lines Sorted by Position
```
GET /data/TSI_ProdBOMLines?$filter=ProdId eq '000123'&$orderby=Position,LineNum
```

### Example Query - Get BOM Lines for Specific Component
```
GET /data/TSI_ProdBOMLines?$filter=ItemId eq 'FOAM-001'
```

### Example Query - Get BOM Lines with Location Info
```
GET /data/TSI_ProdBOMLines?$filter=ProdId eq '000123'&$select=ItemId,ItemName,BOMQty,UnitId,InventLocationId,wMSLocationId
```

## TypeScript Interface

```typescript
export interface TSI_ProdBOMLine {
  ProdId: string;
  ItemId: string;
  LineNum: number;
  BOMQty: number;
  BOMQtySerie: number;
  UnitId: string;
  Position: number;
  ScrapVar: number;
  ItemName: string;
  NameAlias: string;
  ItemGroupId: string;
  Depth: number;
  Width: number;
  Height: number;
  TSIShorteningLength: number;
  TSIBlockWidth: number;
  InventLocationId: string;
  wMSLocationId: string;
  inventDimId: string;
  InventTransId: string;
  InventRefType: number;
  InventRefId: string;
}
```

## Implementation Checklist

- [ ] Verify all custom fields exist in D365
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
    pb.PRODID,
    pb.ITEMID,
    pb.LINENUM,
    pb.BOMQTY,
    pb.BOMQTYSERIE,
    pb.UNITID,
    pb.POSITION,
    pb.SCRAPVAR,
    pt.NAME AS ITEMNAME,
    it.NAMEALIAS,
    iig.ITEMGROUPID,
    it.DEPTH,
    it.WIDTH,
    it.HEIGHT,
    tsi.TSISHORTENINGLENGTH,
    tsi.TSIBLOCKWIDTH,
    id.INVENTLOCATIONID,
    id.WMSLOCATIONID,
    pb.INVENTDIMID,
    pb.INVENTTRANSID,
    pb.INVENTREFTYPE,
    pb.INVENTREFID
FROM ProdBOM pb
INNER JOIN InventTable it
    ON pb.ITEMID = it.ITEMID
INNER JOIN EcoResProduct p
    ON it.PRODUCT = p.RECID
LEFT JOIN EcoResProductSystemLanguage psl
    ON p.RECID = psl.PRODUCT
LEFT JOIN EcoResProductTranslation pt
    ON p.RECID = pt.PRODUCT
    AND psl.LanguageId = pt.SystemLanguageId
LEFT JOIN InventItemGroupItem iig
    ON it.ITEMID = iig.ITEMID
    AND it.DATAAREAID = iig.ITEMDATAAREAID
LEFT JOIN TSIInventTable tsi
    ON it.ITEMID = tsi.TSIITEMID
INNER JOIN InventDim id
    ON pb.INVENTDIMID = id.INVENTDIMID
WHERE pb.PRODID = '000123'
ORDER BY pb.POSITION, pb.LINENUM
```

## Notes

- This is a read-only entity for querying production BOM lines
- Returns all BOM lines for production orders
- Item names support multi-language via EcoResProductTranslation
- ItemGroupId comes from InventItemGroupItem relation table
- Custom dimension fields (ShorteningLength, BlockWidth) from TSIInventTable extension
- Entity includes both warehouse (InventLocationId) and WMS location (WMSLocationId)
- Useful for MES material requirements and consumption tracking
