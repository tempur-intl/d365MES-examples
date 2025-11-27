# D365 Service Bus Event Consumer Sample

This sample demonstrates how to consume D365 business events from Azure Service Bus, specifically focusing on production order events for MES integration.

## Overview

D365 publishes business events to Azure Service Bus when important actions occur. This sample shows how to:
- Receive production order released and status changed events
- Process messages from Service Bus Topics or Queues
- Handle message failures with dead letter queue (DLQ)
- Run in poll-once mode (for testing) or continuous listening mode (for production)

## Configuration

1. Copy `appsettings.Example.json` to `appsettings.Development.json`
2. Update the connection details:

```json
{
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

### Configuration Options

- **ConnectionString**: Azure Service Bus connection string
- **EntityType**: `Topic` (with subscription) or `Queue`
- **TopicName**: Name of the Service Bus topic (if using topics)
- **SubscriptionName**: Name of the subscription (if using topics)
- **QueueName**: Name of the queue (if using queues)
- **MaxDeliveryCount**: Max attempts before sending to DLQ (default: 3)
- **PrefetchCount**: Number of messages to prefetch (default: 10)
- **MaxWaitTimeSeconds**: How long to wait for messages (default: 30)

## Azure Service Bus Setup

### 1. Create Service Bus Resources

1. **Create Service Bus Namespace** (if not already done)
   - Go to Azure Portal → Create Resource → Service Bus
   - Choose **Standard** or **Premium** tier (required for Topics)
   - Note the connection string from **Shared access policies** → **RootManageSharedAccessKey**

2. **Create Topic**
   - In your Service Bus namespace → **Topics** → **+ Topic**
   - **Name**: `businessevents`
   - Keep default settings (1 GB max size, 14 days TTL)
   - Click **Create**

3. **Create Subscription**
   - Click on the `businessevents` topic → **Subscriptions** → **+ Subscription**
   - **Name**: `mes-subscription`
   - **Max delivery count**: `3`
   - **Lock duration**: `5 minutes`
   - **Enable dead lettering on message expiration**: Yes
   - Click **Create**

4. **Add SQL Filter to Subscription**
   - Click on `mes-subscription` → **Filters**
   - Delete the default filter (if exists) or click **+ Add filter**
   - **Filter name**: `DF-MES`
   - **Filter type**: **SQL filter**
   - **SQL filter expression**:
     ```sql
     EventId = 'ProductionOrderReleasedBusinessEvent' AND LegalEntity = '500'
     ```
   - Click **Create**This filter ensures your MES only receives production order events for legal entity 500.

## D365 Business Events Setup

### 1. Configure Business Events Endpoint

1. Go to **System administration > Setup > Business events > Endpoints**
2. Click **New** to add an endpoint
3. **Endpoint type**: Select **Azure Service Bus Topic**
4. **Endpoint name**: `AzureServiceBusTopic` (or any descriptive name)
5. **Topic name**: `businessevents` (must match your Azure topic)
6. **Connection string**: Paste your Service Bus connection string from Azure Portal
7. Click **Test connection** to verify
8. Click **OK** to save

### 2. Activate Business Events

1. Go to **System administration > Setup > Business events > Business events catalog**
2. Search for and activate:
   - `ProductionOrderReleasedBusinessEvent`
3. Go to **System administration > Setup > Business events > Business events**
4. For each activated event:
   - Select the event
   - Click **Manage** or **Endpoints**
   - Assign your Azure Service Bus endpoint
   - Set **Legal entity** filter if needed (optional, since your subscription already filters)
5. Click **Activate**

Events will now be published to your Service Bus topic when production orders are released.

## Usage

### Poll Once (Testing Mode)

Run once and process available messages:

```bash
dotnet run
```

Process up to 20 messages:

```bash
dotnet run -- --max-messages 20
```

### Continuous Listening (Production Mode)

Run continuously and listen for new events:

```bash
dotnet run -- --continuous
```

Press `Ctrl+C` to stop gracefully.

### Check Dead Letter Queue

Inspect messages that failed processing:

```bash
dotnet run -- --check-dlq
```

## Event Types

### Production Order Released Event

Published when a production order is released to the shop floor:

```json
{
  "MessageId": "abc123",
  "EventId": "ProductionOrderReleasedBusinessEvent",
  "EventType": "ProductionOrderReleasedBusinessEvent",
  "EventTime": "2025-11-26T10:30:00Z",
  "LegalEntity": "500",
  "EventData": {
    "ProductionOrderNumber": "P000001234",
    "ItemNumber": "83107273",
    "ProductionSiteId": "01",
    "ProductionWarehouseId": "010",
    "ScheduledStartDate": "2025-11-27T08:00:00Z",
    "ScheduledEndDate": "2025-11-27T16:00:00Z",
    "ProductionOrderStatus": "Released",
    "RemainingSchedulingQuantity": 100
  },
  "DeliveryCount": 1,
  "EnqueuedTime": "2025-11-26T10:30:01Z",
  "Success": true
}
```

## Error Handling

### Automatic Retries

Messages that fail processing are automatically retried:
- First attempt: Process message
- Failure: Abandon message (returns to queue)
- Retry with exponential backoff
- After `MaxDeliveryCount` attempts: Move to Dead Letter Queue

### Dead Letter Queue

Failed messages are moved to the DLQ after max retries. Reasons include:
- Invalid message format
- Deserialization errors
- Processing exceptions
- Business logic errors

Inspect DLQ messages with `--check-dlq` flag to:
- Identify recurring issues
- Fix data quality problems
- Reprocess messages manually if needed

## Best Practices

1. **Use Topics for Multiple Consumers**: If you have multiple systems (MES, warehouse, analytics) that need the same events, use Topics with separate subscriptions
2. **Filter Subscriptions**: Add SQL filters on subscriptions to only receive relevant events (e.g., specific sites or production pools)
3. **Idempotency**: Handle duplicate messages gracefully - D365 may send the same event multiple times
4. **Monitor DLQ**: Regularly check the dead letter queue for recurring failures
5. **Graceful Shutdown**: Use Ctrl+C in continuous mode to shut down gracefully and complete in-flight messages

## Troubleshooting

**No messages received:**
- Check Service Bus connection string
- Verify topic/subscription or queue name
- Ensure D365 business events are activated and configured
- Check subscription filters aren't blocking messages

**Messages going to DLQ:**
- Check application logs for error details
- Inspect DLQ with `--check-dlq` flag
- Verify message schema matches expected format
- Check for breaking changes in D365 event schema

**Connection errors (Connection reset by peer):**
- **Network Restrictions**: Check if Service Bus namespace has network restrictions enabled
  - In Azure Portal → Service Bus Namespace → Networking
  - If "Selected networks" or "Private endpoint" is enabled, you may need VPN/ExpressRoute
  - Try switching to "Public endpoint (all networks)" for testing
- **Firewall/Proxy**: Ensure outbound TCP traffic is allowed on ports 5671 (AMQP over TLS) and 443 (WebSocket fallback)
- **Connection String**: Verify connection string has "Listen" permission (SharedAccessKeyName should have receive rights)
- **Namespace Status**: Check Service Bus namespace isn't disabled or in throttled state
- **DNS Resolution**: Ensure `*.servicebus.windows.net` resolves correctly
- **Retry**: Connection errors may be transient - the SDK will automatically retry

**Test connectivity:**
```bash
# Test DNS resolution
nslookup intl-go.servicebus.windows.net

# Test port connectivity (if telnet is available)
telnet intl-go.servicebus.windows.net 5671
```
