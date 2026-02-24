# Integrated Event-Driven Sample

This sample demonstrates a complete event-driven integration workflow combining Service Bus events with OData queries. It shows how to:

1. **Receive** a `TSIProductionOrderReleasedToMESBusinessEvent` from Azure Service Bus
2. **Extract** the `ProductionOrderNumber` and `Resource` from the event
3. **Query** D365 OData API to get job details using TSI_Jobs entity
4. **Retrieve** related BOM lines using TSI_ProdBOMLines entity

## 📋 Prerequisites

- Azure Service Bus topic with subscription (see `ServiceBusEvents.Samples` for setup)
- D365 Business Events configured to publish `TSIProductionOrderReleasedToMESBusinessEvent`
- Azure AD app registration with D365 API permissions
- .NET 8.0 SDK

## ⚙️ Configuration

Copy `appsettings.Example.json` to `appsettings.Development.json` and configure:

```json
{
  "AzureAd": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  },
  "D365": {
    "BaseUrl": "https://your-environment.operations.dynamics.com",
    "OrganizationId": "your-legal-entity-id"
  },
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=...",
    "EntityType": "Topic",
    "TopicName": "businessevents",
    "SubscriptionName": "mes-subscription",
    "MaxDeliveryCount": 3,
    "PrefetchCount": 10,
    "MaxWaitTimeSeconds": 30
  }
}
```

## 🚀 Usage

### Process Events (Default: 10 messages)

```bash
cd samples/IntegratedEventDriven.Samples
DOTNET_ENVIRONMENT=Development dotnet run
```

### Process Specific Number of Messages

```bash
DOTNET_ENVIRONMENT=Development dotnet run 5
```

## 🔄 How It Works

### 1. Event Arrives from D365

When a production order is released in D365, a business event is published:

```json
{
  "BusinessEventId": "ProductionOrderReleasedBusinessEvent",
  "EventTime": "/Date(1764240678000)/",
  "LegalEntity": "500",
  "ProductionOrderNumber": "10001191",
  "ProductionOrderType": "Standard"
}
```

### 2. Extract Production Order Number

The service parses the event and extracts `ProductionOrderNumber: 10001191`.

### 3. Query OData with Filter

The service makes OData requests with filters:

```http
GET /data/TSI_Jobs?$filter=ProdId eq '10001191' and dataAreaId eq '500'
Authorization: Bearer {token}
```

**Key Point**: The service uses `ProdId` (which equals `ProductionOrderNumber` in D365) to query job details.

### 4. Get Related Data

The service also queries BOM lines with a filter:

```http
GET /data/TSI_ProdBOMLines?$filter=ProdId eq '10001191' and dataAreaId eq '500'
Authorization: Bearer {token}
```

### 5. Process and Complete

The service logs all details and marks the Service Bus message as complete.

## 📊 Sample Output

```
=== Processing Message 4eba91037fcbf011bbd37c1e52617b2f ===
Event: ProductionOrderReleasedBusinessEvent for Production Order: 10001191

Job Details (2 jobs):
  - Job J0001: Item 83107273 at Work Center WC001
  - Job J0002: Item 83107273 at Work Center WC002

BOM Lines (11 materials):
  • 19503: 24 p1
  • 44173: 24 p1
  • 35357: 24 p1
  • 49001: 3.34 kg
  • 14903: 4 p1
  ... and 6 more

✓ Message processed successfully
```

## 🎯 Integration Benefits

This pattern demonstrates:

1. **Event-Driven Architecture**: React to D365 events in real-time
2. **MES-Optimized Queries**: Use TSI entities designed specifically for MES integration
3. **Job-Level Details**: Get detailed job information for production scheduling
4. **Material Visibility**: Immediately see what materials are needed when an order is released

## 🔍 OData Filtering

The sample shows proper OData filtering syntax:

- **Single field**: `ProductionOrderNumber eq '10001191'`
- **Multiple conditions**: `... and dataAreaId eq '500'`
- **Top N results**: `&$top=1`
- **URL encoding**: Filters are properly escaped for HTTP

This is much more efficient than:
- Retrieving all production orders and filtering in code
- Making multiple API calls
- Downloading unnecessary data

## 🛠️ Error Handling

- **Parse failures**: Abandon message for retry
- **OData failures**: Log error but complete message (event still processed)
- **Max retries**: Messages moved to Dead Letter Queue after 3 attempts
- **Graceful shutdown**: Ctrl+C completes in-flight messages

## 💡 Use Cases

This integration pattern is ideal for:

1. **MES Systems**: Receive release event → Query jobs → Schedule production operations
2. **Work Center Systems**: Receive release event → Query jobs → Assign work to specific machines
3. **Labor Tracking**: Receive release event → Query jobs → Track time and progress
4. **Quality Systems**: Receive release event → Query jobs → Schedule inspections

## 🔗 Related Samples

- **ServiceBusEvents.Samples**: Pure event consumption (no OData)
- **OData.Samples**: Pure OData queries (no events)
- **This Sample**: Combined event + OData workflow

## 📚 Further Reading

- [D365 Business Events](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/business-events/home-page)
- [D365 OData API](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/data-entities/odata)
- [OData Query Options](https://www.odata.org/getting-started/basic-tutorial/#queryData)
- [Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
