# D365 MES Integration Samples

This project demonstrates how to integrate a Manufacturing Execution System (MES) with Dynamics 365 Supply Chain Management using the standard MES integration API.

## 📋 Overview

The D365 MES Integration API enables third-party MES systems to communicate production events to D365. This sample demonstrates:

- ✅ Starting production orders
- ✅ Reporting material consumption (picking lists)
- ✅ Reporting time consumption (route cards)
- ✅ Reporting production as finished
- ✅ Ending production orders

## 🔐 Authentication

Uses standard Azure AD OAuth 2.0 authentication with the D365 API scope. The `D365.Auth` library handles this via `D365TokenProvider`.

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
    "EnvironmentId": "your-environment-id",
    "BaseUrl": "https://your-instance.operations.dynamics.com",
    "OrganizationId": "500"
  }
}
```

### Sample Data Configuration

Edit `sample-data.json` with your actual production order data:

```json
{
  "productionOrderNumber": "10001147",
  "startedQuantity": 1,
  "automaticBOMConsumptionRule": "FlushingPrincip",
  "automaticRouteConsumptionRule": "Always",
  "materialConsumption": [
    {
      "itemNumber": "20821",
      "consumptionBOMQuantity": 2,
      "operationNumber": 10,
      "productionSiteId": "01",
      "productionWarehouseId": "010"
    }
  ],
  "reportAsFinished": {
    "itemNumber": "83107273",
    "reportedGoodQuantity": 1,
    "reportedErrorQuantity": 0,
    "productionSiteId": "01",
    "productionWarehouseId": "010",
    "automaticBOMConsumptionRule": "FlushingPrincip",
    "automaticRouteConsumptionRule": "Always",
    "endJob": "No",
    "generateLicensePlate": "Yes",
    "printLabel": "Yes"
  }
}
```

**Benefits**:
- ✅ Easily test with real production orders from your D365 environment
- ✅ No code changes needed to test different scenarios
- ✅ JSON file can be committed to source control as examples

## 🚀 Running the Samples

```bash
cd samples/MesIntegration.Samples
dotnet run
```

## 📝 MES Integration Workflow

The samples simulate a complete production order lifecycle for mattress manufacturing:

### 1. Start Production Order

```csharp
// Data loaded from sample-data.json
var message = new StartProductionOrderMessage
{
    ProductionOrderNumber = sampleData.ProductionOrderNumber,
    StartedQuantity = sampleData.StartedQuantity,
    StartedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
    AutomaticBOMConsumptionRule = sampleData.AutomaticBOMConsumptionRule,
    AutomaticRouteConsumptionRule = sampleData.AutomaticRouteConsumptionRule
};

await mesService.StartProductionOrderAsync(message);
```

**Use Case**: MES notifies D365 when production begins on an assembly line.

### 2. Report Material Consumption

```csharp
var message = new MaterialConsumptionMessage
{
    ProductionOrderNumber = "10001147",
    PickingListLines = new List<PickingListLine>
    {
        new()
        {
            ItemNumber = "20821",
            ConsumptionBOMQuantity = 100,
            OperationNumber = 10,
            ProductionSiteId = "1",
            ProductionWarehouseId = "11"
        }
    }
};

await mesService.ReportMaterialConsumptionAsync(message);
```

**Use Case**: Track raw material (foam blocks, fabric) consumed during production operations.

### 3. Report Time Consumption

```csharp
var message = new RouteCardMessage
{
    ProductionOrderNumber = "10001147",
    RouteCardLines = new List<RouteCardLine>
    {
        new()
        {
            OperationNumber = 10,
            Hours = 2.5m,
            GoodQuantity = 100,
            ErrorQuantity = 0,
            OperationsResourceId = "CuttingMachine-01",
            Worker = "Worker123"
        }
    }
};

await mesService.ReportTimeConsumptionAsync(message);
```

**Use Case**: Track labor hours and machine time for costing and scheduling.

### 4. Report As Finished

```csharp
var message = new ReportAsFinishedMessage
{
    ProductionOrderNumber = "10001147",
    ReportFinishedLines = new List<ReportFinishedLine>
    {
        new()
        {
            ItemNumber = "83107273",
            ReportedGoodQuantity = 98,
            ReportedErrorQuantity = 2,
            ReportAsFinishedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            ProductionWarehouseLocationId = "FINISHED",
            ItemBatchNumber = $"BATCH-{DateTime.UtcNow:yyyyMMdd}"
        }
    }
};

await mesService.ReportAsFinishedAsync(message);
```

**Use Case**: Report completed units and scrap, move finished goods to warehouse.

### 5. End Production Order

```csharp
var message = new EndProductionOrderMessage
{
    ProductionOrderNumber = "10001147",
    EndedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
    AutoUpdate = "Yes"
};

await mesService.EndProductionOrderAsync(message);
```

**Use Case**: Close the production order when all operations are complete.

## 🏭 Memory Foam Manufacturing Scenario

This sample simulates producing 100 queen-size memory foam mattresses:

1. **Start**: Begin production of 100 units
2. **Material**: Consume 100 foam blocks and 100 fabric covers
3. **Operations**:
   - Cutting: 2.5 hours on CuttingMachine-01
   - Assembly: 3 hours on AssemblyLine-01 (98 good, 2 scrap)
4. **Finish**: Report 98 good units, 2 scrap units
5. **End**: Close the production order

## 🔍 Monitoring Messages

View message processing status in D365:

**Production control → Setup → Manufacturing execution → Manufacturing execution systems integration**

Monitor for:
- ✅ Success: Message processed correctly
- ⏳ Processing: Message in queue
- ❌ Failed: Review error details and retry

## 🛠️ Best Practices

1. **Sequential Processing**: Messages for the same production order are processed in order
2. **Error Handling**: Failed messages can be retried up to 3 times automatically
3. **Message IDs**: Not required (unlike IVA), but helpful for tracking
4. **Dates**: Use ISO 8601 format (`yyyy-MM-dd`) for dates
5. **Quantities**: Match BOM quantities for material consumption
6. **Operation Numbers**: Reference existing route operations

## 📊 Integration Patterns

### Real-Time Integration
Send messages immediately as events occur on the shop floor.

### Batch Integration
Collect events during a shift and send in bulk at shift end.

### Hybrid Approach
Send critical events (start/end) immediately, batch consumption data.

## 🔐 Required Permissions

Your Azure AD app registration needs:
- **API Permissions**: Dynamics 365 Finance and Operations (user_impersonation)
- **D365 Security**: Manufacturing execution user role

## 📚 Additional Resources

- [MES Integration Documentation](https://learn.microsoft.com/en-us/dynamics365/supply-chain/production-control/mes-integration)
- [Message Processor](https://learn.microsoft.com/en-us/dynamics365/supply-chain/message-processor/message-processor)
- [Production Orders](https://learn.microsoft.com/en-us/dynamics365/supply-chain/production-control/production-process-overview)

## ⚠️ Troubleshooting

**"Failed to send MES message"**
- Verify Azure AD authentication is working
- Check that the production order exists in D365
- Ensure the Time and attendance license key is enabled

**Messages stuck in "Processing"**
- Check D365 batch job is running (Manufacturing execution systems integration)
- Review System administration → Batch jobs

**"Production order not found"**
- Verify the production order number is correct
- Ensure the order status allows the operation (e.g., can't report finished on Released order)

**Material consumption failures**
- Verify item numbers exist in D365
- Check that warehouse and location are valid
- Ensure BOM lines exist for the items
