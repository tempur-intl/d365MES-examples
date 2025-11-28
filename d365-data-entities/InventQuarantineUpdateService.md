# MES Batch Disposition Tracking

## Overview

**Entity Name**: `ItemBatch` (Standard D365 Entity)
**Purpose**: Track inventory batches with disposition codes for quarantine management
**Primary Use**: MES system filtering batches by disposition code to identify quarantined inventory
**Decision**: Use existing standard entity instead of creating custom entity

## Business Logic Change

**AX2009 Approach**: Used `InventQuarantineOrder` table with STATUS and TRANSTYPE filtering
**D365 Approach**: Uses **batch disposition codes** on inventory batches to mark quarantined stock

The D365 implementation will not use quarantine orders. Instead, inventory will be flagged as quarantined by assigning a specific batch disposition code (e.g., "QUARANTINE") to the batch.

## Legacy SQL View (AX2009)

```sql
CREATE VIEW [dbo].[InventQuarantineUpdateServiceTempur]
AS
  SELECT
    dbo.BYOD_InventQuarantineOrderStaging.TRANSREFID AS ProductionId,
    dbo.BYOD_InventDimStaging.INVENTBATCHID,
    dbo.BYOD_InventQuarantineOrderStaging.QTY
  FROM dbo.BYOD_InventQuarantineOrderStaging
  INNER JOIN dbo.BYOD_InventDimStaging
    ON dbo.BYOD_InventQuarantineOrderStaging.INVENTDIMID = dbo.BYOD_InventDimStaging.INVENTDIMID
  WHERE (dbo.BYOD_InventQuarantineOrderStaging.STATUS = 0 OR
         dbo.BYOD_InventQuarantineOrderStaging.STATUS = 1)
    AND (dbo.BYOD_InventQuarantineOrderStaging.TRANSTYPE = 8 OR
         dbo.BYOD_InventQuarantineOrderStaging.TRANSTYPE = 2)
```

## D365 Table Mapping

| Legacy Table | Legacy Purpose | D365 Alternative | D365 Approach |
|-------------|----------------|------------------|---------------|
| `InventQuarantineOrder` | Track quarantine orders | `InventBatch` | Track batch disposition codes |
| `InventDim` | Batch identification | `InventBatch` | Batches are primary records |

## Standard Entity: ItemBatch

### Entity Information

**Standard D365 Entity**: `ItemBatch`
**Public Name**: `ItemBatches`
**Table**: `InventBatch`
**Purpose**: Batch master with disposition codes

### Key Fields for Quarantine Tracking

| Field Name | Data Type | Description | Required |
|-----------|-----------|-------------|----------|
| `dataAreaId` | String | Company identifier | Yes |
| `ItemNumber` | String | Item number | Yes |
| `BatchNumber` | String | Batch number | Yes |
| `BatchDispositionCode` | String | **Disposition code** (e.g., "QUARANTINE") | No |
| `BatchDescription` | String | Batch description | No |
| `ManufacturingDate` | DateTime | Manufacturing date | No |
| `BatchExpirationDate` | DateTime | Expiration date | No |
| `BestBeforeDate` | DateTime | Best before date | No |
| `VendorBatchNumber` | String | Vendor batch number | No |

### Primary Key

- `dataAreaId`
- `ItemNumber`
- `BatchNumber`

### Filter Strategy

Query batches where `BatchDispositionCode` equals your quarantine disposition code (e.g., "QUARANTINE", "HOLD", or whatever code is configured in D365).

## Entity Properties (Standard Entity)

**No custom entity required** - Use the existing `ItemBatch` entity.

```
General:
  - Name: ItemBatch (Standard)
  - Label: Item Batch
  - Public Entity Name: ItemBatches
  - Public Collection Name: ItemBatches
  - Is Public: Yes

Data Management:
  - Entity Category: Master
  - Data Management Enabled: Yes

OData:
  - Is Read Only: No (but MES will only read)
```

## OData Endpoint

### Base URL
```
https://<your-d365-instance>.operations.dynamics.com/data/ItemBatches
```

### Example Queries

#### Get all quarantined batches (by disposition code)
```
GET /data/ItemBatches?$filter=dataAreaId eq '500' and BatchDispositionCode eq 'QUARANTINE'
```

#### Get quarantined batch for specific item
```
GET /data/ItemBatches?$filter=dataAreaId eq '500' and ItemNumber eq 'ITEM123' and BatchDispositionCode eq 'QUARANTINE'
```

#### Get specific batch details
```
GET /data/ItemBatches?$filter=dataAreaId eq '500' and ItemNumber eq 'ITEM123' and BatchNumber eq 'BATCH001'
```

#### Get all batches with specific disposition code (with additional fields)
```
GET /data/ItemBatches?$filter=dataAreaId eq '500' and BatchDispositionCode eq 'QUARANTINE'&$select=ItemNumber,BatchNumber,BatchDispositionCode,ManufacturingDate,BatchExpirationDate
```

## Integration Examples

### TypeScript Integration

#### Service Method

```typescript
// In D365ApiService or similar
async getQuarantinedBatches(filters?: {
  itemNumber?: string;
  batchNumber?: string;
  dispositionCode?: string;
}): Promise<ItemBatch[]> {
  const endpoint = `${this.d365BaseUrl}/data/ItemBatches`;

  const dispositionCode = filters?.dispositionCode || 'QUARANTINE'; // Default or configurable
  const filterParts: string[] = [
    `dataAreaId eq '${this.dataAreaId}'`,
    `BatchDispositionCode eq '${dispositionCode}'`
  ];

  if (filters?.itemNumber) {
    filterParts.push(`ItemNumber eq '${filters.itemNumber}'`);
  }

  if (filters?.batchNumber) {
    filterParts.push(`BatchNumber eq '${filters.batchNumber}'`);
  }

  const response = await this.makeAuthenticatedRequest<{ value: ItemBatch[] }>(
    'GET',
    endpoint,
    undefined,
    {
      $filter: filterParts.join(' and '),
      $select: 'ItemNumber,BatchNumber,BatchDispositionCode,BatchDescription,ManufacturingDate,BatchExpirationDate'
    }
  );

  return response.value || [];
}

// Check if a specific batch is quarantined
async isBatchQuarantined(itemNumber: string, batchNumber: string): Promise<boolean> {
  const endpoint = `${this.d365BaseUrl}/data/ItemBatches`;

  const response = await this.makeAuthenticatedRequest<{ value: ItemBatch[] }>(
    'GET',
    endpoint,
    undefined,
    {
      $filter: `dataAreaId eq '${this.dataAreaId}' and ItemNumber eq '${itemNumber}' and BatchNumber eq '${batchNumber}'`,
      $select: 'BatchDispositionCode',
      $top: 1
    }
  );

  const batch = response.value?.[0];
  return batch?.BatchDispositionCode === 'QUARANTINE'; // Or check against configured code
}
```

#### Interface

```typescript
export interface ItemBatch {
  dataAreaId: string;
  ItemNumber: string;
  BatchNumber: string;
  BatchDispositionCode?: string; // e.g., "QUARANTINE", "AVAILABLE", "HOLD"
  BatchDescription?: string;
  ManufacturingDate?: string; // ISO DateTime
  BatchExpirationDate?: string; // ISO DateTime
  BestBeforeDate?: string; // ISO DateTime
  VendorBatchNumber?: string;
  PrimaryVendorOriginCountryRegionId?: string;
  AreBatchAttributesInherited?: boolean;
  IsBatchConsolidated?: boolean;
}
```

#### Configuration

```typescript
// config/d365.config.ts
export const d365Config = {
  baseUrl: process.env.D365_BASE_URL,
  dataAreaId: process.env.D365_DATA_AREA_ID,
  quarantineDispositionCode: process.env.D365_QUARANTINE_DISPOSITION_CODE || 'QUARANTINE',
  // ... other config
};
```

### C# Integration

#### Service Class

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class D365BatchService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _dataAreaId;
    private readonly string _quarantineDispositionCode;

    public D365BatchService(
        HttpClient httpClient,
        string baseUrl,
        string dataAreaId,
        string quarantineDispositionCode = "QUARANTINE")
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
        _dataAreaId = dataAreaId;
        _quarantineDispositionCode = quarantineDispositionCode;
    }

    public async Task<List<ItemBatch>> GetQuarantinedBatchesAsync(
        string itemNumber = null,
        string batchNumber = null,
        string dispositionCode = null)
    {
        var endpoint = $"{_baseUrl}/data/ItemBatches";
        var code = dispositionCode ?? _quarantineDispositionCode;

        var filterParts = new List<string>
        {
            $"dataAreaId eq '{_dataAreaId}'",
            $"BatchDispositionCode eq '{code}'"
        };

        if (!string.IsNullOrEmpty(itemNumber))
        {
            filterParts.Add($"ItemNumber eq '{itemNumber}'");
        }

        if (!string.IsNullOrEmpty(batchNumber))
        {
            filterParts.Add($"BatchNumber eq '{batchNumber}'");
        }

        var filter = string.Join(" and ", filterParts);
        var selectFields = "ItemNumber,BatchNumber,BatchDispositionCode,BatchDescription,ManufacturingDate,BatchExpirationDate";
        var queryString = $"?$filter={Uri.EscapeDataString(filter)}&$select={selectFields}";

        var response = await _httpClient.GetAsync(endpoint + queryString);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ODataResponse<ItemBatch>>(content);

        return result?.Value ?? new List<ItemBatch>();
    }

    public async Task<bool> IsBatchQuarantinedAsync(string itemNumber, string batchNumber)
    {
        var endpoint = $"{_baseUrl}/data/ItemBatches";
        var filter = $"dataAreaId eq '{_dataAreaId}' and ItemNumber eq '{itemNumber}' and BatchNumber eq '{batchNumber}'";
        var queryString = $"?$filter={Uri.EscapeDataString(filter)}&$select=BatchDispositionCode&$top=1";

        var response = await _httpClient.GetAsync(endpoint + queryString);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ODataResponse<ItemBatch>>(content);

        var batch = result?.Value?.FirstOrDefault();
        return batch?.BatchDispositionCode == _quarantineDispositionCode;
    }
}
```

#### Model Classes

```csharp
public class ItemBatch
{
    [JsonProperty("dataAreaId")]
    public string DataAreaId { get; set; }

    [JsonProperty("ItemNumber")]
    public string ItemNumber { get; set; }

    [JsonProperty("BatchNumber")]
    public string BatchNumber { get; set; }

    [JsonProperty("BatchDispositionCode")]
    public string BatchDispositionCode { get; set; }

    [JsonProperty("BatchDescription")]
    public string BatchDescription { get; set; }

    [JsonProperty("ManufacturingDate")]
    public DateTime? ManufacturingDate { get; set; }

    [JsonProperty("BatchExpirationDate")]
    public DateTime? BatchExpirationDate { get; set; }

    [JsonProperty("BestBeforeDate")]
    public DateTime? BestBeforeDate { get; set; }

    [JsonProperty("VendorBatchNumber")]
    public string VendorBatchNumber { get; set; }

    [JsonProperty("PrimaryVendorOriginCountryRegionId")]
    public string PrimaryVendorOriginCountryRegionId { get; set; }

    [JsonProperty("AreBatchAttributesInherited")]
    public bool? AreBatchAttributesInherited { get; set; }

    [JsonProperty("IsBatchConsolidated")]
    public bool? IsBatchConsolidated { get; set; }
}

public class ODataResponse<T>
{
    [JsonProperty("@odata.context")]
    public string Context { get; set; }

    [JsonProperty("value")]
    public List<T> Value { get; set; }
}
```

#### Configuration (appsettings.json)

```json
{
  "D365": {
    "BaseUrl": "https://your-instance.operations.dynamics.com",
    "DataAreaId": "500",
    "QuarantineDispositionCode": "QUARANTINE"
  }
}
```

#### Dependency Injection Setup

```csharp
// In Startup.cs or Program.cs
services.AddHttpClient<D365BatchService>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = config["D365:BaseUrl"];
    var dataAreaId = config["D365:DataAreaId"];
    var dispositionCode = config["D365:QuarantineDispositionCode"];

    // Configure authentication (OAuth2)
    var tokenProvider = serviceProvider.GetRequiredService<ID365TokenProvider>();
    var token = tokenProvider.GetAccessToken().Result;
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
});
```

## Batch Disposition Codes

Disposition codes are configured in D365 at **Inventory management > Setup > Batch > Batch disposition master**.

Common disposition codes:
| Code | Description | Typical Use |
|------|-------------|-------------|
| `AVAILABLE` | Available for use | Normal inventory |
| `QUARANTINE` | Quarantined | Failed quality checks, held for review |
| `HOLD` | On hold | Temporary hold pending approval |
| `REJECTED` | Rejected | Cannot be used |
| `BLOCKED` | Blocked | Administratively blocked |

**Important**: Verify the actual disposition code used in your D365 instance. The code is configurable and may differ from the examples above.

## Testing

### Verify Disposition Codes in D365

```sql
-- Check available disposition codes
SELECT DISPOSITIONCODE, NAME, INVENTSTATUSAVAILABLE, INVENTSTATUSBLOCKED
FROM InventBatchDispositionMaster
WHERE DATAAREAID = '500'

-- Check batches with specific disposition
SELECT
    ITEMID,
    INVENTBATCHID,
    PDSBATCHATTRIBID AS DispositionCode,
    PRODDATE AS ManufacturingDate,
    EXPDATE AS ExpirationDate,
    DATAAREAID
FROM InventBatch
WHERE PDSBATCHATTRIBID = 'QUARANTINE'
  AND DATAAREAID = '500'
```

### Test OData Endpoint

```bash
# Get all quarantined batches
curl -X GET \
  "https://<instance>.operations.dynamics.com/data/ItemBatches?\$filter=dataAreaId%20eq%20'500'%20and%20BatchDispositionCode%20eq%20'QUARANTINE'&\$top=10" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json"

# Get specific batch
curl -X GET \
  "https://<instance>.operations.dynamics.com/data/ItemBatches?\$filter=dataAreaId%20eq%20'500'%20and%20ItemNumber%20eq%20'ITEM123'%20and%20BatchNumber%20eq%20'BATCH001'" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json"
```

## Deployment Checklist

**No custom entity creation needed** - Using standard `ItemBatch` entity.

### D365 Configuration
- [ ] Configure batch disposition codes in D365 (**Inventory management > Setup > Dimensions > Batch disposition master**)
- [ ] Define "QUARANTINE" (or equivalent) disposition code
- [ ] Configure inventory status rules for quarantined batches
- [ ] Set up batch disposition workflows (if needed)
- [ ] Test batch disposition assignment via UI

### MES Integration
- [ ] Verify `ItemBatch` entity is accessible via OData
- [ ] Test OData queries with disposition code filters
- [ ] Update MES configuration with quarantine disposition code value
- [ ] Modify service layer to query `ItemBatches` instead of custom entity
- [ ] Update TypeScript interfaces
- [ ] Implement `getQuarantinedBatches()` method
- [ ] Implement `isBatchQuarantined()` method
- [ ] Update any UI displaying quarantine information
- [ ] Test integration end-to-end

### Security
- [ ] Grant read access to `ItemBatch` entity for MES service account
- [ ] Test OData access with service account credentials

## Security

### Required Privileges

- Read access to `InventBatch` table
- Execute permission on `ItemBatch` entity
- View permissions for batch disposition codes

## Performance Considerations

- Standard D365 indexes on `InventBatch` table include:
  - `ItemId + InventBatchId` (primary key)
  - `dataAreaId`
- Consider creating index on `PDSBATCHATTRIBID` (disposition code field) if high query volume
- Use `$select` to limit returned fields
- Use `$top` for pagination on large result sets
- Cache disposition code configuration in MES

## Notes

- **Standard entity** - No custom development required
- Batch disposition codes must be configured in D365 before use
- MES will filter by disposition code rather than querying quarantine orders
- Read-only access sufficient for MES integration
- Disposition codes are master data - changes should be rare
- This approach aligns with D365 best practices for batch tracking
