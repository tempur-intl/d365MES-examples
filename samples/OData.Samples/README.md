# D365 OData Query Samples

This project demonstrates how to query reference data from Dynamics 365 Supply Chain Management using OData endpoints.

## 📋 Overview

D365 exposes extensive data through OData v4 endpoints. This sample demonstrates querying TSI custom entities for MES integration:

- ✅ TSI Items (MES item master)
- ✅ TSI Production BOM Lines (MES BOM data)
- ✅ TSI Labels (MES label printing with logos)
- ✅ TSI Jobs (MES production jobs)
- ✅ Warehouse Work Lines (warehouse operations)
- ✅ Item Batches (batch tracking and quarantine)

## 🔐 Authentication

Uses standard Azure AD OAuth 2.0 authentication with your D365 instance scope. The `D365.Auth` library handles this via `D365TokenProvider`.

## ⚙️ Configuration

### Authentication Configuration

Update `appsettings.json` (or `appsettings.Development.json` for local development):

```json
{
  "AzureAd": {
    "TenantId": "00000000-0000-0000-0000-000000000000",
    "ClientId": "00000000-0000-0000-0000-000000000000",
    "ClientSecret": "your-secret-here"
  },
  "D365": {
    "BaseUrl": "https://your-instance.operations.dynamics.com",
    "OrganizationId": "500"
  }
}
```

### Sample Queries Configuration

Edit `sample-queries.json` with your actual data identifiers:

```json
{
  "tsiItems": {
    "itemId": "83110290"
  },
  "tsiProdBomLines": {
    "prodId": "10002664"
  },
  "tsiLabels": {
    "prodId": "10002664",
    "udiUnit": "x1"
  },
  "tsiJobs": {
    "prodId": "10002664"
  },
  "warehouseWorkLines": {
    "filter": "dataAreaId eq '500' and WarehouseWorkStatus eq Microsoft.Dynamics.DataEntities.WHSWorkStatus'Open' and ItemNumber eq '83300244'",
    "top": 10
  },
  "itemBatches": {
    "filter": "dataAreaId eq '500' and BatchDispositionCode eq 'QUARANTINE'",
    "top": 10
  }
}
```

**Note**: For enum filters use the fully-qualified OData enum format, e.g. `Microsoft.Dynamics.DataEntities.WHSWorkStatus'Open'`.

**Benefits**:
- ✅ Test queries against real data in your D365 environment
- ✅ No code changes needed to query different items/orders
- ✅ Easy to share query examples with team members

## 🚀 Running the Samples

```bash
cd samples/OData.Samples
dotnet run
```

## 📝 Sample Queries

### 1. Query TSI Items

```csharp
var items = await odataService.GetTsiItemsAsync(sampleQueries.TsiItems.ItemId);

foreach (var item in items)
{
    Console.WriteLine($"Item: {item.ItemId} - {item.ItemName}");
    Console.WriteLine($"  Group: {item.ItemGroupId}");
    Console.WriteLine($"  BOM Unit: {item.BOMUnitId}");
}
```

**Use Case**: Load item master data into MES system.

### 2. Query TSI Production BOM Lines

```csharp
var bomLines = await odataService.GetTsiProdBomLinesAsync(sampleQueries.TsiProdBomLines.ProdId);

foreach (var line in bomLines)
{
    Console.WriteLine($"BOM Line: {line.ItemId} - {line.ItemName}");
    Console.WriteLine($"  Quantity: {line.BOMQty} {line.UnitId}");
    Console.WriteLine($"  Location: {line.InventLocationId}");
}
```

**Use Case**: Get detailed BOM information with inventory locations for MES material tracking.

### 3. Query TSI Labels

```csharp
var labels = await odataService.GetTsiLabelsAsync(
    prodId: sampleQueries.TsiLabels.ProdId,
    udiUnit: sampleQueries.TsiLabels.UDIUnit);

foreach (var label in labels)
{
    Console.WriteLine($"Label: {label.ProdId} - {label.LabelItemId}");
    Console.WriteLine($"  EAN: {label.LabelEAN_Code}");
    Console.WriteLine($"  UDI: {label.HasUDI} ({label.UDIUnit})");
    if (label.Logos != null)
    {
        Console.WriteLine($"  Logos: {label.Logos.Count}");
        foreach (var logo in label.Logos)
        {
            Console.WriteLine($"    - {logo.TSILogoId}: {logo.TSILogoPath} (pos: {logo.TSILogoPosition})");
        }
    }
}
```

**Use Case**: Retrieve label printing data with EAN/UDI codes and automatically expanded logo information for MES label printing.

### 4. Query TSI Jobs

```csharp
var jobs = await odataService.GetTsiJobsAsync(sampleQueries.TsiJobs.ProdId);

foreach (var job in jobs)
{
    Console.WriteLine($"Job: {job.JobId} - {job.ItemId}");
    Console.WriteLine($"  Work Center: {job.WrkCtrId}");
    Console.WriteLine($"  Consumption: {job.ItemNameConsumption}");
}
```

**Use Case**: Load production job details for MES work instructions and tracking.

### 5. Query Warehouse Work Lines

```csharp
var workLines = await odataService.GetWarehouseWorkLinesAsync(
    filter: sampleQueries.WarehouseWorkLines.Filter,
    top: sampleQueries.WarehouseWorkLines.Top);

foreach (var line in workLines)
{
    Console.WriteLine($"Work: {line.WorkId} - Line {line.LineNumber}");
    Console.WriteLine($"  Item: {line.ItemNumber}");
    Console.WriteLine($"  Quantity: {line.WorkQuantity}");
    Console.WriteLine($"  Location: {line.WMSLocationId}");
    Console.WriteLine($"  Status: {line.WarehouseWorkStatus}");
}
```

**Use Case**: Monitor warehouse operations and work progress in MES system.

### 6. Query Item Batches

```csharp
var batches = await odataService.GetItemBatchesAsync(
    filter: sampleQueries.ItemBatches.Filter,
    top: sampleQueries.ItemBatches.Top);

foreach (var batch in batches)
{
    Console.WriteLine($"Batch: {batch.ItemNumber} - {batch.BatchNumber}");
    Console.WriteLine($"  Disposition: {batch.BatchDispositionCode}");
    Console.WriteLine($"  Manufacturing Date: {batch.ManufacturingDate}");
    Console.WriteLine($"  Expiration Date: {batch.BatchExpirationDate}");
    Console.WriteLine($"  Quantity: {batch.PhysicalInventoryQuantity}");
}
```

**Use Case**: Track batch information and quarantine status for quality control in MES.

## 🔍 OData Query Syntax

### Filtering

```csharp
// Single condition
filter = "dataAreaId eq '500'";

// Multiple conditions (AND)
filter = "dataAreaId eq '500' and ItemNumber eq '83300244'";

// Enum value filter
filter = "WarehouseWorkStatus eq Microsoft.Dynamics.DataEntities.WHSWorkStatus'Open'";

// Comparison operators
filter = "WorkQuantity gt 10"; // greater than
```

### Selecting Fields

```csharp
select = "WorkId,ItemNumber,WorkQuantity,WarehouseWorkStatus";
```

### Top N Results

```csharp
top = 10; // Return first 10 records
```

## 🏭 Integration Patterns

### Master Data Sync
Periodically query and cache product, BOM, and route data in MES database.

### Real-Time Lookups
Query D365 when operator selects a production order in MES.

### Validation
Verify materials exist before allowing material consumption entry.

## 📊 Common OData Entities

| Entity | Use Case |
|--------|----------|
| `TSI_Items` | MES item master data |
| `TSI_ProdBOMLines` | MES production BOM with warehouse locations |
| `TSI_Labels` | MES label printing data (includes expanded Logos navigation) |
| `TSI_Jobs` | MES production jobs |
| `WarehouseWorkLines` | Open warehouse work / pick operations |
| `InventBatchTableV2` | Batch tracking and quarantine status |

## 🛠️ Best Practices

1. **Use Filters**: Always filter queries to reduce data transfer
2. **Select Fields**: Use `$select` to return only needed fields
3. **Pagination**: Use `$top` and `$skip` for large datasets
4. **Caching**: Cache master data locally to reduce API calls
5. **Error Handling**: Handle HTTP 404 (not found) gracefully
6. **Rate Limiting**: Implement throttling for bulk operations

## 🔐 Required Permissions

Your Azure AD app registration needs:
- **API Permissions**: Dynamics 365 Finance and Operations (user_impersonation)
- **D365 Security**: Read permissions for queried entities

## 📚 Additional Resources

- [OData Documentation](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/data-entities/odata)
- [Data Entities](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/data-entities/data-entities-data-packages)
- [OData Query Options](https://learn.microsoft.com/en-us/odata/concepts/queryoptions-overview)

## ⚠️ Troubleshooting

**"401 Unauthorized"**
- Verify Azure AD token scope matches D365 instance URL
- Check app registration has correct API permissions
- Ensure user has read access to entity in D365

**"404 Not Found"**
- Verify entity name is correct (case-sensitive)
- Check that data exists with your filter criteria
- Ensure entity is exposed via OData (not all entities are)

**"400 Bad Request"**
- Check OData filter syntax
- Verify field names are correct (case-sensitive)
- Ensure date formats are ISO 8601

**Slow Queries**
- Add filters to reduce result set
- Use `$select` to return fewer fields
- Check D365 database indexes
- Consider caching frequently accessed data

## 🔍 Discovering Available Entities

To see all available OData entities:

```
GET https://your-instance.operations.dynamics.com/data/$metadata
```

Or browse the OData endpoint in a browser:
```
https://your-instance.operations.dynamics.com/data
```

## 🎯 Manufacturing-Specific Entities

For memory foam bed manufacturing, focus on:

- **TSI_Items** - MES-optimized item master data
- **TSI_ProdBOMLines** - MES BOM data with warehouse locations
- **TSI_Labels** - Label printing data with EAN/UDI codes (includes expanded Logos navigation)
- **TSI_Jobs** - Production job details for MES tracking
- **WarehouseWorkLines** - Open warehouse work and pick operations
- **InventBatchTableV2** - Batch disposition and quarantine tracking
