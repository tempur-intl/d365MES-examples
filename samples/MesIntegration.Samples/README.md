# D365 MES Integration Samples

This project demonstrates how to integrate a Manufacturing Execution System (MES) with Dynamics 365 Supply Chain Management using the standard MES integration API.

## 📋 Overview

The D365 MES Integration API enables third-party MES systems to communicate production events to D365. This sample demonstrates:

- ✅ Starting production orders
- ✅ Reporting material consumption (picking lists)
- ✅ Reporting production as finished
- ✅ Ending production orders
- ✅ Creating warehouse movement work (return raw materials to warehouse)
- ✅ Creating inventory count journals (`TSIInventCountJournal`)
- ✅ Updating batch disposition codes (`TSIUpdateBatchDisposition`)

## 📚 Documentation

For detailed parameter mappings and API reference, see:
- [MES Integration API Parameters Documentation](../../d365-data-entities/MES_Integration_API_Parameters.md)
- [Microsoft MES Integration Documentation](https://learn.microsoft.com/en-us/dynamics365/supply-chain/production-control/mes-integration)

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
  "automaticBOMConsumptionRule": "Never",
  "automaticRouteConsumptionRule": "Never",
  "pickingListJournalNameId": "Pick",
  "routeCardJournalNameId": "Route",
  "postNow": "Yes",
  "endPickingList": "No",
  "materialConsumption": [
    {
      "itemNumber": "20821",
      "consumptionBOMQuantity": 2,
      "consumptionBOMQuantityUnit": "p1",
      "operationNumber": 10,
      "productionSiteId": "01",
      "productionWarehouseId": "010",
      "consumptionDate": "2024-01-15",
      "itemBatchNumber": "BATCH001",
      "inventoryLotId": "LOT001"
    }
  ],
  "reportAsFinished": {
    "itemNumber": "83107273",
    "reportedGoodQuantity": 50,
  },
  "movementWork": {
    "licensePlate": "LP-2024-001",
    "sourceLocation": "Aisle01-Rack01-Shelf01",
    "destinationLocation": "",
    "quantity": 0,
    "itemId": ""
  },
  "inventCountJournal": {
    "productionOrderNumber": "10001147",
    "itemNumber": "20821",
    "location": "Aisle01-Rack01-Shelf01",
    "licensePlate": "LP-2024-001",
    "countedQuantity": 10,
    "countDate": "2024-01-15"
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

### 3. Report As Finished

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

### 4. End Production Order

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

### 5. Create Movement Work

> **⚠️ Architectural note**: This operation uses a **separate standalone D365 service** (`TSIMesWebServices/TSIMesWebService/process`) rather than the message queue used by operations 1–4. The call is **synchronous** — D365 processes the request immediately and returns a result (or error) before the HTTP response completes. This was designed intentionally so that MES receives instant feedback on work creation failures, rather than discovering them later through queue error monitoring.

```csharp
var contract = new MovementWorkContract
{
    LicensePlate = "LP-2024-001",       // Required — the only mandatory field
    SourceLocation = "Aisle01-Rack01-Shelf01", // Optional
    DestinationLocation = "",            // Optional
    Quantity = 0,                        // Optional, defaults to 0
    ItemId = ""                          // Optional
    // DataAreaId is omitted here — MovementWorkService fills it from config
};

var result = await movementWorkService.CreateMovementWorkAsync(contract);
// result == "Created" on success
```

**Use Case**: Return unused raw materials from the production floor back to the warehouse after a production order completes or is cancelled.

**Request payload sent to D365**:
```json
{
  "_contract": {
    "LicensePlate": "LP-2024-001",
    "DataAreaId": "500",
    "SourceLocation": "Aisle01-Rack01-Shelf01",
    "DestinationLocation": "",
    "Quantity": 0,
    "ItemId": ""
  }
}
```

**Response**: HTTP 200 with the string `Created`.

### 6. Create Inventory Count Journal

```csharp
var message = new InventCountJournalMessage
{
    ProductionOrderNumber = "10001147",
    ItemNumber = "20821",
    Location = "Aisle01-Rack01-Shelf01",
    LicensePlate = "LP-2024-001",
    CountedQuantity = "10",
    CountDate = "2024-01-15"
};

await mesService.CreateInventCountJournalAsync(message);
```

**Use Case**: MES reports a physical count of inventory at a specific location/license plate to D365, which creates an inventory counting journal line. This is typically used after spot checks during production.

**Request payload sent to D365**:
```json
{
  "_companyId": "500",
  "_messageQueue": "JmgMES3P",
  "_messageType": "TSIInventCountJournal",
  "_messageContent": "{\"ProductionOrderNumber\":\"10001147\",\"ItemNumber\":\"20821\",\"Location\":\"Aisle01-Rack01-Shelf01\",\"LicensePlate\":\"LP-2024-001\",\"CountedQuantity\":\"10\",\"CountDate\":\"2024-01-15\"}"
}
```

> This message is **queue-based** (same as operations 1–4). Monitor processing status via **Production control → Setup → Manufacturing execution → Manufacturing execution systems integration**.

### 7. Update Batch Disposition

```csharp
var message = new UpdateBatchDispositionMessage
{
    ProductionOrderNumber = "10001147",
    ItemNumber = "20821",
    BatchNumber = "BATCH001",
    DispositionCode = "Available"
};

await mesService.UpdateBatchDispositionAsync(message);
```

**Use Case**: After quarantine inspection or quality review, MES notifies D365 to change a batch's disposition code (e.g., from `Quarantine` to `Available`), making the inventory available for use or sale.

**Request payload sent to D365**:
```json
{
  "_companyId": "500",
  "_messageQueue": "JmgMES3P",
  "_messageType": "TSIUpdateBatchDisposition",
  "_messageContent": "{\"ProductionOrderNumber\":\"10001147\",\"ItemNumber\":\"20821\",\"BatchNumber\":\"BATCH001\",\"DispositionCode\":\"Available\"}"
}
```

> This message is **queue-based** (same as operations 1–4 and 6). Monitor processing status via **Production control → Setup → Manufacturing execution → Manufacturing execution systems integration**.

## 🏭 Memory Foam Manufacturing Scenario

This sample simulates producing 100 queen-size memory foam mattresses:

1. **Start**: Begin production of 100 units
2. **Material**: Consume 100 foam blocks and 100 fabric covers
3. **Operations**:
   - Cutting: 2.5 hours on CuttingMachine-01
   - Assembly: 3 hours on AssemblyLine-01 (98 good, 2 scrap)
4. **Finish**: Report 98 good units, 2 scrap units
5. **End**: Close the production order
6. **Return**: Create movement work to return unused raw materials to the warehouse
7. **Count**: Create an inventory count journal line to reconcile on-hand quantities at a specific location/license plate
8. **Disposition**: Update the batch disposition code to release inventory from quarantine

## 🔍 Monitoring Messages

View message processing status in D365 for operations 1–4, 6, and 7 (queue-based):

**Production control → Setup → Manufacturing execution → Manufacturing execution systems integration**

Monitor for:
- ✅ Success: Message processed correctly
- ⏳ Processing: Message in queue
- ❌ Failed: Review error details and retry

> **Movement work (operation 5)** does not go through this queue. Errors surface immediately as HTTP error responses from `MovementWorkService`, so no async monitoring is needed.
>
> **Inventory count journal (operation 6)** is queue-based and will appear in the message processor alongside operations 1–4.
>
> **Batch disposition update (operation 7)** is queue-based and will appear in the message processor alongside operations 1–4 and 6.

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
- [Message Processor Developer Guide](https://learn.microsoft.com/en-us/dynamics365/supply-chain/message-processor/developer/message-processor-develop)
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

**"Failed to create movement work"**
- The error response body from D365 contains the specific reason — check the logs
- Verify the license plate exists and has on-hand inventory
- Confirm the source location is valid for the warehouse
- Ensure the D365 warehouse management processes (WMS) are enabled for the warehouse
- This call is synchronous — D365 validates and creates work in real time, so transient D365 load issues may also cause failures
