# Architecture Overview

This document explains the architecture and design decisions for the D365 Integration Samples.

## 🏗️ High-Level Architecture

```
┌────────────────────────────────────────────────────────────────────────┐
│                       MES Vendor Application                           │
│                     (Your Manufacturing Software)                      │
└──┬──────────┬────────────┬────────────┬────────────────────────────────┘
   │          │            │            │
   │          │            │            │
┌──▼────┐ ┌───▼───────┐ ┌──▼─────┐ ┌────▼────────┐
│  IVA  │ │   MES     │ │ OData  │ │ Service Bus │
│Sample │ │Integration│ │Queries │ │   Events    │
│       │ │  Sample   │ │ Sample │ │   Sample    │
└──┬────┘ └──┬────────┘ └──┬─────┘ └────┬────────┘
   │         │             │            │
   └─────────┴─────────────┘            │
             │                          │
        ┌────▼─────┐                    │
        │D365.Auth │                    │
        │ Library  │                    │
        └────┬─────┘                    │
             │                          │
   ┌─────────┴─────────┐                │
   │                   │                │
┌──▼──────┐    ┌───────▼────────┐  ┌────▼──────────────┐
│Azure AD │    │   D365 SCM     │  │  Azure Service    │
│ OAuth   │    │  Environment   │  │      Bus          │
└─────────┘    └────────┬───────┘  │ (Business Events) │
                        │          └───────────────────┘
                        │
                ┌───────▼────────┐
                │  Business Event│
                │   Publisher    │
                └────────────────┘
```

## 🔐 Authentication Architecture

### Shared Authentication Library (D365.Auth)

The `D365.Auth` library provides three token providers:

1. **AzureAdTokenProvider** (Base)
   - Acquires Azure AD OAuth 2.0 tokens
   - Used by both IVA and D365 token providers
   - Implements token caching with expiration tracking

2. **IvaTokenProvider** (Inventory Visibility)
   - Two-step authentication:
     - Step 1: Get Azure AD token (scope: IVA service)
     - Step 2: Exchange for IVA access token
   - Handles 307 redirects automatically

3. **D365TokenProvider** (Standard APIs)
   - Single-step authentication
   - Uses Azure AD token directly
   - Scope: D365 instance URL

### Why Separate Token Providers?

Different authentication flows require different implementations:

| Provider | OAuth Flow | Target API |
|----------|-----------|------------|
| AzureAdTokenProvider | Client credentials | Azure AD |
| IvaTokenProvider | Two-step exchange | Inventory Visibility |
| D365TokenProvider | Client credentials | D365 OData/Message Service |

## 📦 Project Structure

### Source Projects

```
src/
└── D365.Auth/
    ├── Models/
    │   ├── AuthModels.cs        # Public configuration models
    │   └── InternalModels.cs    # Internal token response models
    └── Providers/
        ├── AzureAdTokenProvider.cs
        ├── IvaTokenProvider.cs
        └── D365TokenProvider.cs
```

### Sample Projects

```
samples/
├── InventoryVisibility.Samples/
│   ├── Models/
│   │   └── IvaModels.cs         # IVA request/response models
│   ├── Services/
│   │   └── IvaService.cs        # IVA API client
│   ├── Program.cs               # Sample scenarios
│   └── README.md
├── MesIntegration.Samples/
│   ├── Models/
│   │   └── MesModels.cs         # MES message models
│   ├── Services/
│   │   └── MesService.cs        # MES API client
│   ├── Program.cs               # Production lifecycle
│   └── README.md
├── OData.Samples/
│   ├── Models/
│   │   └── ODataModels.cs       # D365 entity models
│   ├── Services/
│   │   └── ODataService.cs      # OData query client
│   ├── Program.cs               # Query examples
│   └── README.md
└── ServiceBusEvents.Samples/
    ├── Models/
    │   └── BusinessEventModels.cs  # D365 business event models
    ├── Services/
    │   └── ServiceBusConsumerService.cs  # Service Bus client
    ├── Program.cs               # Event consumer
    └── README.md
```

## 🔄 Request Flow

### Inventory Visibility Request Flow

```
MES Application
    │
    ▼
IvaService.QueryOnHandAsync()
    │
    ▼
IvaTokenProvider.GetIvaTokenAsync()
    │
    ├─► AzureAdTokenProvider.GetTokenAsync()  [Step 1]
    │   └─► Azure AD → Azure AD Token
    │
    └─► POST to IVA Security Service          [Step 2]
        └─► Security Service → IVA Access Token
    │
    ▼
POST to IVA API
    └─► Inventory Visibility Service → Response
```

### MES Integration Request Flow

```
MES Application
    │
    ▼
MesService.StartProductionOrderAsync()
    │
    ▼
D365TokenProvider.GetD365TokenAsync()
    │
    └─► AzureAdTokenProvider.GetTokenAsync()
        └─► Azure AD → Azure AD Token
    │
    ▼
POST to D365 Message Service
    │
    ▼
D365 Message Processor
    │
    └─► Production Order Updated
```

### OData Query Flow

```
MES Application
    │
    ▼
ODataService.GetProductionOrdersAsync()
    │
    ▼
D365TokenProvider.GetD365TokenAsync()
    │
    └─► AzureAdTokenProvider.GetTokenAsync()
        └─► Azure AD → Azure AD Token
    │
    ▼
GET from D365 OData Endpoint
    │
    └─► D365 Data Service → JSON Response
```

### Service Bus Event Flow

```
D365 Production Event
    │
    ▼
Business Event Publisher
    │
    ▼
Azure Service Bus Topic
    │
    ├─► Subscription 1 (Line 1) → SQL Filter
    ├─► Subscription 2 (Line 2) → SQL Filter
    └─► Subscription 3 (Line 3) → SQL Filter
    │
    ▼
ServiceBusConsumerService.ReceiveMessagesAsync()
    │
    ├─► PeekLock Message
    ├─► Deserialize BusinessEventEnvelope
    ├─► Parse Event Type
    │   ├─► ProductionOrderReleasedEvent
    │   └─► ProductionOrderStatusChangedEvent
    │
    ├─► Process Message
    │   ├─► Success → CompleteMessageAsync()
    │   ├─► Max Retries → DeadLetterMessageAsync()
    │   └─► Transient Error → AbandonMessageAsync()
    │
    ▼
MES Application Updated
```

## 🎯 Design Patterns

### 1. Dependency Injection

All services use constructor injection:

```csharp
public class IvaService
{
    private readonly HttpClient _httpClient;
    private readonly IvaTokenProvider _tokenProvider;
    private readonly ILogger<IvaService> _logger;

    public IvaService(
        HttpClient httpClient,
        IvaTokenProvider tokenProvider,
        ILogger<IvaService> logger)
    {
        // ...
    }
}
```

**Benefits**:
- Testability (mock dependencies)
- Loose coupling
- Configuration flexibility

### 2. Token Caching with Thread Safety

```csharp
private TokenResponse? _cachedToken;
private readonly SemaphoreSlim _lock = new(1, 1);

public async Task<string> GetTokenAsync()
{
    await _lock.WaitAsync();
    try
    {
        if (_cachedToken != null && !_cachedToken.IsExpired)
            return _cachedToken.AccessToken;

        // Acquire new token
    }
    finally
    {
        _lock.Release();
    }
}
```

**Benefits**:
- Prevents multiple simultaneous token requests
- Reduces API calls
- Improves performance

### 3. Configuration as Code

```csharp
services
    .AddSingleton(configuration.GetSection("AzureAd").Get<AzureAdConfig>()!)
    .AddScoped<AzureAdTokenProvider>()
    .AddScoped<IvaTokenProvider>();
```

**Benefits**:
- Type-safe configuration
- Easy to test
- Clear dependencies

### 4. Service Layer Pattern

Each API has a dedicated service:
- `IvaService` - Inventory Visibility operations
- `MesService` - MES message sending
- `ODataService` - D365 entity queries

**Benefits**:
- Single responsibility
- Reusable across applications
- Easy to extend

## 🔧 Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Language | C# | Latest |
| Framework | .NET | 8.0 |
| HTTP Client | HttpClient | Built-in |
| JSON | System.Text.Json | 8.0 |
| DI Container | Microsoft.Extensions.DependencyInjection | 8.0 |
| Logging | Microsoft.Extensions.Logging | 8.0 |
| Configuration | Microsoft.Extensions.Configuration | 8.0 |
| Service Bus | Azure.Messaging.ServiceBus | Latest |

## 📊 Data Flow

### Outbound (MES → D365)

```
Production Event in MES
    ↓
Create Message Model
    ↓
MesService.SendMessageAsync()
    ↓
D365 Message Service API
    ↓
Message Processor (Batch Job)
    ↓
D365 Database Updated
```

### Inbound (D365 → MES)

```
Query Triggered in MES
    ↓
ODataService.QueryAsync()
    ↓
D365 OData API
    ↓
Parse Response
    ↓
Display in MES UI
```

### Bidirectional (Inventory)

```
MES Produces Item
    ↓
IvaService.PostOnHandChangeAsync()
    ↓
Inventory Visibility Service
    ↓

MES Queries Stock
    ↓
IvaService.QueryOnHandAsync()
    ↓
Inventory Visibility Service
```

### Event-Driven (Service Bus)

```
D365 Production Order Released
    ↓
Business Event Published
    ↓
Service Bus Topic
    ↓
Subscription (with SQL Filter)
    ↓
ServiceBusConsumerService
    ↓
Process Event in MES
    ↓
(Optional) Send Response to D365
```

## 🔒 Security Considerations

### 1. Secrets Management
- Client secrets stored in `appsettings.json`
- **Production**: Use Azure Key Vault
- Never commit secrets to source control

### 2. Token Security
- Tokens cached in memory only
- Automatic expiration handling
- HTTPS required for all APIs

### 3. Least Privilege
- Azure AD permissions: Only what's needed
- D365 security roles: Minimal required access
- Audit logs: Track all API calls

## 🚀 Performance Optimizations

### 1. Token Caching
- Tokens cached until 5 minutes before expiration
- Reduces Azure AD calls
- Thread-safe implementation

### 2. HttpClient Reuse
- Single HttpClient instance per service
- Registered with DI container
- Prevents socket exhaustion

### 3. Async/Await
- All I/O operations are async
- Non-blocking operations
- Better scalability

## 🧪 Testing Strategy

### Unit Testing
Test token providers with mocked HttpClient:
```csharp
var mockHttp = new MockHttpMessageHandler();
mockHttp.When("*/token").Respond("application/json", tokenJson);
```

### Integration Testing
Test against D365 sandbox environment:
```csharp
[Fact]
public async Task CanQueryProductionOrders()
{
    var orders = await _odataService.GetProductionOrdersAsync();
    Assert.NotEmpty(orders);
}
```

### End-to-End Testing
Full workflow testing in development environment.

## 📈 Scalability

### Horizontal Scaling
- Stateless services (except token cache)
- Can run multiple instances
- Load balance with reverse proxy

### Rate Limiting
- Implement exponential backoff
- Respect D365 throttling limits
- Queue messages during peak times

## 🎉 Event-Driven Architecture

### Service Bus Integration

The ServiceBusEvents sample demonstrates event-driven integration with D365:

**Key Features**:
- **Topics with Subscriptions**: Pub-sub pattern for multiple consumers
- **SQL Filters**: Server-side filtering by EventId, LegalEntity, Site, etc.
- **PeekLock Mode**: Message processing with completion acknowledgment
- **Automatic Retries**: MaxDeliveryCount with Dead Letter Queue
- **Multiple Operation Modes**: Poll once (testing), continuous (production), DLQ inspection

**Architecture Benefits**:
1. **Decoupling**: MES reacts to D365 events without polling
2. **Scalability**: Each assembly line has its own subscription
3. **Reliability**: Automatic retries and dead letter queue
4. **Filtering**: Only receive relevant events per line/site
5. **Independence**: One line failure doesn't affect others

**Message Flow**:
```
D365 Event → Service Bus Topic → Subscription Filter → MES Consumer
                                      ↓ (if failed)
                                 Dead Letter Queue
```

## 🔮 Future Enhancements

Potential improvements:

1. ~~**Business Events Integration**~~ ✅ **COMPLETED**
   - ✅ Subscribe to D365 business events via Service Bus
   - ✅ Real-time notifications to MES
   - ✅ Per-line subscriptions with SQL filters

2. **Batch Processing**
   - Bulk insert/update operations
   - Scheduled synchronization jobs

3. **Enhanced Error Handling**
   - Exponential backoff for transient errors
   - Alerting on dead letter queue growth
   - Automatic replay from DLQ after fixes

4. **Monitoring**
   - Application Insights integration
   - Custom metrics and alerts
   - Service Bus metrics tracking

5. **Caching Layer**
   - Redis for distributed caching
   - Cache master data locally

## 📚 References

- [Azure AD OAuth 2.0](https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)
- [D365 OData](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/data-entities/odata)
- [Inventory Visibility API](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-visibility-api)
- [MES Integration](https://learn.microsoft.com/en-us/dynamics365/supply-chain/production-control/mes-integration)
- [D365 Business Events](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/business-events/home-page)
- [Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview)
