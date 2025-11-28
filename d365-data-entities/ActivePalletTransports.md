# MES Active Pallet Transport Entity

## Overview

**Standard D365 Entity**: `WarehouseWorkLines`
**Purpose**: Expose active pallet transports with expedition status less than 10
**Primary Use**: MES system tracking of pallet movements

## Legacy SQL View

```sql
CREATE VIEW [dbo].[ActivePalletTransports]
AS
  SELECT LicensePlateId AS WmsPalletId, ExpeditionStatus
  FROM dbo.BYOD_WMSTransport
  WHERE (ExpeditionStatus < 10)
```

## D365 Table Mapping & Solution

| Legacy Table (AX2009) | D365 Table | D365 Data Entity (Already Exists!) |
|-------------|-----------|-----------------------------------|
| `WMSTransport` | `WHSWorkLine` | **`WarehouseWorkLines`** |

### ✅ RECOMMENDED SOLUTION: Use Existing `WarehouseWorkLines` Entity

**D365 already has a standard OData entity called **`WarehouseWorkLines`** that likely covers our needs:

**Entity Properties**:
- **Public Entity Name**: `WarehouseWorkLines`
- **Base Table**: `WHSWorkLine`
- **Key Fields**:
  - `LicensePlateNumber` ✓
  - `WarehouseWorkStatus` (replaces `ExpeditionStatus`)
  - `WarehouseWorkId`
  - `ItemNumber`
  - `RemainingWorkQuantity`
  - `WarehouseLocationId`
  - Plus 40+ other fields

**Migration Path**:
- AX2009 `WMSTransport.LicensePlateId` → D365 `WarehouseWorkLines.LicensePlateNumber`
- AX2009 `WMSTransport.ExpeditionStatus` → D365 `WarehouseWorkLines.WarehouseWorkStatus`

**⚠️ Decision Point**:
1. **Option A (RECOMMENDED)**: Use the existing `WarehouseWorkLines` entity directly
   - ✅ Already deployed and tested in D365
   - ✅ Maintained by Microsoft
   - ✅ No development required
   - ⚠️ May include more data than you need (filter appropriately)

2. **Option B (NOT RECOMMENDED)**: Create custom entity (unnecessary when standard entity exists)
   - ⚠️ Requires development and testing
   - ✅ Can be tailored to exact MES needs
   - ✅ Can join additional custom fields if needed

## Using Existing WarehouseWorkLines Entity

### OData Endpoint (Already Available!)
```
https://<your-instance>.operations.dynamics.com/data/WarehouseWorkLines
```

### Field Mapping

| Legacy Field (AX2009) | D365 Field | Notes |
|----------------------|------------|-------|
| `LicensePlateId` | `LicensePlateNumber` | Direct mapping |
| `ExpeditionStatus` | `WarehouseWorkStatus` | Enum: Open, Closed, Cancelled, InProcess |
| `dataAreaId` | `dataAreaId` | Company identifier |

### Additional Useful Fields Available

The `WarehouseWorkLines` entity includes many more fields you might find useful:
- `ItemNumber` - Product being handled
- `RemainingWorkQuantity` - Qty left to process
- `WarehouseLocationId` - Current location
- `InventorySiteId` - Site
- `ShipmentId` - Related shipment
- `ContainerId` - Container reference
- `WarehouseWorkId` - Work order ID

### Query to Replace Legacy View

Instead of creating a custom entity, use this OData query:

```
GET /data/WarehouseWorkLines
  ?$filter=dataAreaId eq '500'
    and WarehouseWorkStatus ne Microsoft.Dynamics.DataEntities.WHSWorkStatus'Closed'
    and WarehouseWorkStatus ne Microsoft.Dynamics.DataEntities.WHSWorkStatus'Cancelled'
  &$select=LicensePlateNumber,WarehouseWorkStatus,WarehouseWorkId,ItemNumber
```

This filters for active work (equivalent to `ExpeditionStatus < 10` in AX2009)

### Primary Key

- `WmsPalletId`
- `dataAreaId`

### Indexes

- Primary index on `WmsPalletId`, `dataAreaId`
- Index on `ExpeditionStatus` for filtering

## Entity Properties

```
General:
  - Name: WarehouseWorkLines (Standard D365 Entity)
  - Label: MES Active Pallet Transport
  - Public Entity Name: TSI_ActivePalletTransports
  - Public Collection Name: TSI_ActivePalletTransports
  - Is Public: Yes

Data Management:
  - Entity Category: Reference
  - Data Management Enabled: Yes

OData:
  - Is Read Only: Yes
```

## X++ Code Structure

### Data Entity Query

The entity should implement a query with WHERE clause:

```xpp
// In the entity's init() method or query
public void init()
{
    super();

    // Filter for active work/transports
    // NOTE: Original AX2009 filter was ExpeditionStatus < 10
    // In D365, this likely translates to specific work status values:
    WHSWork_ds.addRange(fieldNum(WHSWork, workStatus))
        .value(strFmt('%1,%2,%3',
            WHSWorkStatus::Open,      // Example: adjust based on your needs
            WHSWorkStatus::InProcess,
            WHSWorkStatus::Completed));

    // Alternative: If you have custom ExpeditionStatus field
    // WHSWork_ds.addRange(fieldNum(WHSWork, ExpeditionStatus))
    //     .value(queryValue(10), queryValue(QueryRangeType::Less));
}
```

### Alternative: Using a View

If you prefer, create a view first:

```xpp
// Note: Using standard WarehouseWorkLines entity
// No custom view required
{
    public void initValue()
    {
        super();
    }
}

// View query in AOT
SELECT WmsPalletId, ExpeditionStatus, dataAreaId
FROM WHSWork  -- Or your equivalent table
WHERE workStatus IN (Open, InProcess, Completed)  -- Adjust as needed
```

## OData Endpoint

### Base URL
```
https://<your-d365-instance>.operations.dynamics.com/data/TSI_ActivePalletTransports
```

### Example Queries

#### Get all active transports for company 500
```
GET /data/TSI_ActivePalletTransports?$filter=dataAreaId eq '500'
```

#### Get specific pallet
```
GET /data/TSI_ActivePalletTransports?$filter=dataAreaId eq '500' and WmsPalletId eq 'PALLET001'
```

#### Get transports with status less than 5
```
GET /data/TSI_ActivePalletTransports?$filter=dataAreaId eq '500' and ExpeditionStatus lt 5
```

## Integration Examples

### TypeScript Integration

#### Service Method

```typescript
// In D365ApiService or similar
async getActivePalletTransports(filters?: {
  wmsPalletId?: string;
  maxStatus?: number;
}): Promise<ActivePalletTransport[]> {
  const endpoint = `${this.d365BaseUrl}/data/WarehouseWorkLines`;

  const filterParts: string[] = [`dataAreaId eq '${this.dataAreaId}'`];

  if (filters?.wmsPalletId) {
    filterParts.push(`LicensePlateNumber eq '${filters.wmsPalletId}'`);
  }

  if (filters?.maxStatus !== undefined) {
    filterParts.push(`WarehouseWorkStatus lt Microsoft.Dynamics.DataEntities.WHSWorkStatus'${filters.maxStatus}'`);
  }

  const response = await this.makeAuthenticatedRequest<{ value: ActivePalletTransport[] }>(
    'GET',
    endpoint,
    undefined,
    { $filter: filterParts.join(' and ') }
  );

  return response.value || [];
}
```

#### Interface

```typescript
export interface ActivePalletTransport {
  LicensePlateNumber: string;
  WarehouseWorkStatus: number;
  ItemNumber: string;
  WarehouseLocationId: string;
  RemainingWorkQuantity: number;
  dataAreaId: string;
}
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

public class D365PalletTransportService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _dataAreaId;

    public D365PalletTransportService(
        HttpClient httpClient,
        string baseUrl,
        string dataAreaId)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
        _dataAreaId = dataAreaId;
    }

    public async Task<List<ActivePalletTransport>> GetActivePalletTransportsAsync(
        string licensePlateNumber = null,
        int? maxStatus = null)
    {
        var endpoint = $"{_baseUrl}/data/WarehouseWorkLines";

        var filterParts = new List<string>
        {
            $"dataAreaId eq '{_dataAreaId}'"
        };

        if (!string.IsNullOrEmpty(licensePlateNumber))
        {
            filterParts.Add($"LicensePlateNumber eq '{licensePlateNumber}'");
        }

        if (maxStatus.HasValue)
        {
            filterParts.Add($"WarehouseWorkStatus lt Microsoft.Dynamics.DataEntities.WHSWorkStatus'{maxStatus.Value}'");
        }

        var filter = string.Join(" and ", filterParts);
        var queryString = $"?$filter={Uri.EscapeDataString(filter)}";

        var response = await _httpClient.GetAsync(endpoint + queryString);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ODataResponse<ActivePalletTransport>>(content);

        return result?.Value ?? new List<ActivePalletTransport>();
    }
}
```

#### Model Class

```csharp
public class ActivePalletTransport
{
    [JsonProperty("LicensePlateNumber")]
    public string LicensePlateNumber { get; set; }

    [JsonProperty("WarehouseWorkStatus")]
    public int WarehouseWorkStatus { get; set; }

    [JsonProperty("ItemNumber")]
    public string ItemNumber { get; set; }

    [JsonProperty("WarehouseLocationId")]
    public string WarehouseLocationId { get; set; }

    [JsonProperty("RemainingWorkQuantity")]
    public decimal RemainingWorkQuantity { get; set; }

    [JsonProperty("dataAreaId")]
    public string DataAreaId { get; set; }
}

public class ODataResponse<T>
{
    [JsonProperty("@odata.context")]
    public string Context { get; set; }

    [JsonProperty("value")]
    public List<T> Value { get; set; }
}
```

## Testing

### Test in D365

1. Navigate to **Data management** > **Data entities**
2. Find `WarehouseWorkLines`
3. Click **Validate** to check configuration
4. Use **Export** to test data retrieval

### Test OData

```bash
# Get OAuth token first (use existing D365TokenService)

# Test endpoint
curl -X GET \
  "https://<instance>.operations.dynamics.com/data/TSI_ActivePalletTransports?\$filter=dataAreaId%20eq%20'500'&\$top=10" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json"
```

## Deployment Checklist

- [ ] **CRITICAL**: Identify which D365 table contains pallet transport data:
  - [ ] Check if `WHSWork` has the needed data
  - [ ] Check if custom `WMSTransport` table was preserved
  - [ ] Check `WHSLoadLine`, `WHSShipmentTable` as alternatives
- [ ] Map AX2009 fields to D365 equivalents:
  - [ ] `LicensePlateId` → Verify field name and location
  - [ ] `ExpeditionStatus` → Map to D365 work status or custom field
- [ ] Translate filter logic (`ExpeditionStatus < 10` → D365 work statuses)
- [ ] Verify `WarehouseWorkLines` entity is accessible
- [ ] Configure entity properties (Is Public = Yes)
- [ ] Add fields and map to data source
- [ ] Set primary key
- [ ] Add query filter for `ExpeditionStatus < 10`
- [ ] Build and synchronize database
- [ ] Test in Data Management workspace
- [ ] Test OData endpoint
- [ ] Grant security permissions to OAuth app
- [ ] Update MES system integration

## Security

### Required Privileges

The OAuth service principal needs:
- Read access to `WarehouseWorkLines` (standard entity)
- Execute permission on OData endpoint
- Access to relevant `dataAreaId` companies

### Setup in D365

1. **System administration** > **Security** > **Security configuration**
2. Verify access to standard entity: `WarehouseWorkLines`
3. Standard entity - verify OAuth app has warehouse access permissions
4. Assign duty to OAuth service principal role

## Notes

- This is a read-only entity (no insert/update/delete)
- The filter `ExpeditionStatus < 10` is critical for performance
- Consider adding an index on `ExpeditionStatus` if queries are slow
- The entity respects D365 security and company boundaries
