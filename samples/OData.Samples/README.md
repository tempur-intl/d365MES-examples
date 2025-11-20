# D365 OData Query Samples

This project demonstrates how to query reference data from Dynamics 365 Supply Chain Management using OData endpoints.

## 📋 Overview

D365 exposes extensive data through OData v4 endpoints. This sample demonstrates querying:

- ✅ Production orders
- ✅ Released products (item master data)
- ✅ Bill of Materials (BOM) lines
- ✅ Route operations
- ✅ Inventory on-hand

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
    "OrganizationId": "usmf"
  }
}
```

### Sample Queries Configuration

Edit `sample-queries.json` with your actual data identifiers:

```json
{
  "productionOrders": {
    "filter": "ProductionOrderStatus eq Microsoft.Dynamics.DataEntities.ProdStatus'Released'",
    "top": 10
  },
  "product": {
    "productNumber": "83107273"
  },
  "bom": {
    "productionOrderNumber": "10001147"
  },
  "route": {
    "productionOrderNumber": "10001147"
  }
}
```

**Note**: For production order status filters, use the enum format: `Microsoft.Dynamics.DataEntities.ProdStatus'StatusValue'` where StatusValue can be Released, Scheduled, StartedUp, ReportedFinished, Ended, etc.

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

### 1. Query Production Orders

```csharp
// Query parameters loaded from sample-queries.json
var orders = await odataService.GetProductionOrdersAsync(
    filter: sampleQueries.ProductionOrders.Filter,
    top: sampleQueries.ProductionOrders.Top);

foreach (var order in orders)
{
    Console.WriteLine($"Order: {order.ProductionOrderNumber}");
    Console.WriteLine($"  Item: {order.ItemNumber}");
    Console.WriteLine($"  Qty: {order.ProductionOrderQuantity}");
    Console.WriteLine($"  Status: {order.ProductionOrderStatus}");
}
```

**Use Case**: Display active production orders in MES system dashboard.

### 2. Query Product Information

```csharp
var product = await odataService.GetProductAsync("83107273");

Console.WriteLine($"Product: {product.ProductName}");
Console.WriteLine($"  Inventory Unit: {product.InventoryUnitSymbol}");
Console.WriteLine($"  BOM Unit: {product.BomUnitSymbol}");
```

**Use Case**: Show product details when selecting items in MES interface.

### 3. Query BOM Lines

```csharp
var bomLines = await odataService.GetBomLinesAsync("BOM-83107273");

foreach (var line in bomLines)
{
    Console.WriteLine($"Line {line.LineNumber}: {line.ItemNumber}");
    Console.WriteLine($"  Quantity: {line.BomQuantity} {line.BomUnitSymbol}");
}
```

**Use Case**: Display required materials for production order in MES.

### 4. Query Route Operations

```csharp
var operations = await odataService.GetRouteOperationsAsync("ROUTE-MATTRESS");

foreach (var op in operations)
{
    Console.WriteLine($"Op {op.OperationNumber}: {op.OperationId}");
    Console.WriteLine($"  Process Time: {op.ProcessTime} min");
    Console.WriteLine($"  Setup Time: {op.SetupTime} min");
}
```

**Use Case**: Load operation sequences and time standards into MES.

### 5. Query Inventory On-Hand

```csharp
var inventory = await odataService.GetInventoryOnHandAsync("FoamSheet-QK", "1");

foreach (var entry in inventory)
{
    Console.WriteLine($"Warehouse: {entry.InventoryWarehouseId}");
    Console.WriteLine($"  Available: {entry.AvailablePhysicalInventoryQuantity}");
    Console.WriteLine($"  Total: {entry.TotalAvailableInventoryQuantity}");
}
```

**Use Case**: Check material availability before starting production.

## 🔍 OData Query Syntax

### Filtering

```csharp
// Single condition
filter = "ProductionOrderStatus eq 'Scheduled'";

// Multiple conditions (AND)
filter = "ItemNumber eq 'A0001' and ProductionSiteId eq '1'";

// Multiple conditions (OR)
filter = "ProductionOrderStatus eq 'Scheduled' or ProductionOrderStatus eq 'Started'";

// Comparison operators
filter = "ProductionOrderQuantity gt 100"; // greater than
filter = "ScheduledStartDate ge 2024-01-01"; // greater than or equal
```

### Selecting Fields

```csharp
select = "ProductionOrderNumber,ItemNumber,ProductionOrderStatus";
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
| `ProductionOrders` | Active production orders |
| `ReleasedProducts` | Item master data |
| `BOMLines` | Bill of materials |
| `RouteOperations` | Production routing |
| `InventoryOnHandEntries` | Available inventory |
| `ProductionOrderLines` | Production order components |
| `WorkCalendarTables` | Shop calendar |
| `InventoryDimensions` | Batch/serial numbers |

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

- **ProductionOrders** - Order status and header info
- **ProductionOrderLines** - BOM components for orders
- **BOMLines** - Standard BOM definitions
- **RouteOperations** - Manufacturing processes
- **ReleasedProducts** - Item specifications
- **InventoryOnHandEntries** - Stock availability
- **ProdBOMJournalLines** - Material consumption history
- **ProdRouteCardJournalLines** - Labor/time reporting history
