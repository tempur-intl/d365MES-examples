# Inventory Visibility Add-in Samples

This project demonstrates how to authenticate with and call the Inventory Visibility Add-in APIs for Dynamics 365 Supply Chain Management.

## 📋 Overview

The Inventory Visibility Add-in provides real-time inventory tracking and management capabilities. This sample demonstrates:

- ✅ Two-step authentication (Azure AD → IVA access token)
- ✅ Querying on-hand inventory
- ✅ Posting inventory change events
- ✅ Creating soft reservations

## 🔐 Authentication Flow

The IVA uses a two-step authentication process:

1. **Step 1**: Acquire an Azure AD token with scope `0cdb527f-a8d1-4bf8-9436-b352c68682b2/.default`
2. **Step 2**: Exchange the Azure AD token for an IVA access token from the security service

The `D365.Auth` library handles this automatically via `IvaTokenProvider`.

## ⚙️ Configuration

Update `appsettings.json` with your environment details:

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
    "OrganizationId": "usmf"
  },
  "InventoryVisibility": {
    "SecurityServiceUrl": "https://securityservice.operations365.dynamics.com",
    "ServiceUrl": "https://inventoryservice.operations365.dynamics.com"
  }
}
```

### Finding Your Configuration Values

- **TenantId**: Azure Portal → Azure Active Directory → Overview → Tenant ID
- **ClientId** & **ClientSecret**: Azure Portal → App Registrations → Your App
- **EnvironmentId**: Lifecycle Services → Environment Details
- **OrganizationId**: Your legal entity (e.g., "usmf")

## 🚀 Running the Samples

```bash
cd samples/InventoryVisibility.Samples
dotnet run
```

## 📝 Sample Operations

### 1. Query On-Hand Inventory

```csharp
var query = new OnHandQueryRequest
{
    Filters = new QueryFilters
    {
        OrganizationId = new List<string> { "usmf" },
        ProductId = new List<string> { "FoamSheet-QK" },
        SiteId = new List<string> { "1" },
        LocationId = new List<string> { "11" }
    },
    GroupByValues = new List<string> { "siteId", "locationId" }
};

var results = await ivaService.QueryOnHandAsync(query);
```

### 2. Post On-Hand Change

```csharp
var changeEvent = new OnHandChangeRequest
{
    Id = $"mes-event-{Guid.NewGuid()}",
    OrganizationId = "usmf",
    ProductId = "FoamSheet-QK",
    Dimensions = new Dictionary<string, string>
    {
        ["siteId"] = "1",
        ["locationId"] = "11"
    },
    Quantities = new Dictionary<string, Dictionary<string, decimal>>
    {
        ["mes"] = new Dictionary<string, decimal>
        {
            ["produced"] = 10
        }
    }
};

await ivaService.PostOnHandChangeAsync(changeEvent);
```

### 3. Create Soft Reservation

```csharp
var reservation = new ReservationRequest
{
    Id = $"reserve-{Guid.NewGuid()}",
    OrganizationId = "usmf",
    ProductId = "FoamSheet-QK",
    Quantity = 5,
    Modifier = "softReservOrdered",
    Dimensions = new Dictionary<string, string>
    {
        ["siteId"] = "1",
        ["locationId"] = "11"
    }
};

var result = await ivaService.CreateReservationAsync(reservation);
```

## 🏭 Manufacturing Use Cases

### Material Tracking
Use on-hand queries to track raw materials (foam blocks, fabric) across production lines.

### Production Reporting
Post on-hand changes when materials are consumed or products are completed on assembly lines.

### Order Fulfillment
Create soft reservations to allocate finished goods for customer orders.

## 🛠️ Best Practices

1. **Use Unique IDs**: Always generate unique IDs for change events to prevent duplicates
2. **Cache Tokens**: The `IvaTokenProvider` caches tokens automatically
3. **Handle 307 Redirects**: The security service may return redirects (handled automatically)
4. **Batch Operations**: Use bulk endpoints when posting multiple changes
5. **Error Handling**: Implement retry logic for transient failures

## 📚 Additional Resources

- [Inventory Visibility API Documentation](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-visibility-api)
- [IVA Authentication Guide](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-visibility-api#inventory-visibility-authentication)
- [IVA Configuration](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-visibility-configuration)

## ⚠️ Troubleshooting

**"Failed to acquire IVA token"**
- Verify your Azure AD app has the correct API permissions
- Check that the environment ID is correct
- Ensure the IVA add-in is installed in your D365 environment

**"401 Unauthorized"**
- Token may have expired - the provider handles refresh automatically
- Verify Azure AD app client secret hasn't expired

**"404 Not Found"**
- Verify the environment ID in configuration
- Check that the IVA service URL is correct for your region
