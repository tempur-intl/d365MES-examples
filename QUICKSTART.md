# Quick Start Guide

Get up and running with D365 integration samples in 15 minutes.

## ⚡ Prerequisites Check

Before starting, ensure you have:

- [ ] .NET 8.0 SDK installed ([Download](https://dotnet.microsoft.com/download))
- [ ] Access to Azure Portal (to create app registration)
- [ ] Access to D365 Supply Chain Management environment
- [ ] Admin rights to grant Azure AD permissions

## 🚀 5-Minute Setup

### Step 1: Create Azure AD App (3 minutes)

1. Go to [Azure Portal](https://portal.azure.com) → **Azure Active Directory** → **App registrations**
2. Click **New registration**:
   - Name: `D365-MES-Integration`
   - Click **Register**
3. Copy these values:
   - **Application (client) ID**
   - **Directory (tenant) ID**
4. Go to **Certificates & secrets** → **New client secret**
   - Copy the **Value** (you can't see it again!)
5. Go to **API permissions** → **Add permission** → **APIs my organization uses**
   - Search "Dynamics" → Select **Dynamics ERP** → **user_impersonation**
   - Click **Grant admin consent**

### Step 2: Get D365 Details (2 minutes)

1. Go to [Lifecycle Services](https://lcs.dynamics.com)
2. Open your environment → Copy **Environment ID**
3. Note your D365 URL: `https://[instance].operations.dynamics.com`
4. Note your legal entity code (e.g., "500")

### Step 3: Clone and Configure (5 minutes)

```bash
# Clone repository
git clone <your-repo-url>
cd D365IntegrationSamples

# Build solution
dotnet build

# Configure samples
cd samples/InventoryVisibility.Samples
```

Edit `appsettings.json`:

```json
{
  "AzureAd": {
    "TenantId": "PASTE-YOUR-TENANT-ID",
    "ClientId": "PASTE-YOUR-CLIENT-ID",
    "ClientSecret": "PASTE-YOUR-CLIENT-SECRET"
  },
  "D365": {
    "EnvironmentId": "PASTE-YOUR-ENVIRONMENT-ID",
    "BaseUrl": "https://YOUR-INSTANCE.operations.dynamics.com",
    "OrganizationId": "500"
  }
}
```

### Step 4: Test (1 minute)

```bash
dotnet run
```

✅ You should see token acquisition messages and API calls!

## 📂 Project Structure

```
D365IntegrationSamples/
├── src/D365.Auth/                    # Shared authentication library
├── samples/
│   ├── InventoryVisibility.Samples/  # IVA API examples
│   ├── MesIntegration.Samples/       # MES message service
│   └── OData.Samples/                # OData queries
├── README.md                          # Overview
├── SETUP.md                          # Detailed setup guide
└── QUICKSTART.md                     # This file
```

## 🎯 What Each Sample Does

### Inventory Visibility Samples
**Run**: `cd samples/InventoryVisibility.Samples && dotnet run`

- Queries on-hand inventory
- Posts inventory changes
- Creates soft reservations

**Use when**: Tracking real-time inventory across production lines

### MES Integration Samples
**Run**: `cd samples/MesIntegration.Samples && dotnet run`

- Starts production orders
- Reports material consumption
- Reports time/labor
- Reports finished goods
- Ends production orders

**Use when**: Sending production events from MES to D365

### OData Query Samples
**Run**: `cd samples/OData.Samples && dotnet run`

- Queries production orders
- Retrieves BOM data
- Gets route operations
- Checks inventory levels

**Use when**: Loading reference data from D365 into MES

## 🔍 Testing with Your Data

### For Inventory Visibility:

Edit `Program.cs` to use your item numbers:

```csharp
ProductId = new List<string> { "YOUR-ITEM-NUMBER" },
SiteId = new List<string> { "YOUR-SITE" },
LocationId = new List<string> { "YOUR-WAREHOUSE" }
```

### For MES Integration:

Use an existing production order:

```csharp
var productionOrderNumber = "YOUR-PROD-ORDER-NUMBER";
```

### For OData Queries:

Use your product/BOM/route IDs:

```csharp
var product = await odataService.GetProductAsync("YOUR-ITEM");
var bomLines = await odataService.GetBomLinesAsync("YOUR-BOM-ID");
```

## ❗ Common Issues

### "Failed to acquire token"
- Check ClientId, ClientSecret, TenantId are correct
- Verify admin consent was granted

### "401 Unauthorized"
- Register Azure AD app in D365: **System administration** → **Azure Active Directory applications**
- Assign security role to the app user

### "404 Not Found" for IVA
- Verify Inventory Visibility add-in is installed
- Check EnvironmentId is correct

### MES messages not processing
- Enable **Time and attendance** license key in D365
- Check batch job is running: **System administration** → **Batch jobs**

## 📚 Next Steps

1. **Read detailed docs**: See [SETUP.md](./SETUP.md) for complete setup
2. **Review samples**: Understand the code in each Program.cs
3. **Customize**: Adapt samples for your manufacturing processes
4. **Test thoroughly**: Use a development environment first
5. **Monitor**: Check D365 message processor and logs

## 🏭 Memory Foam Manufacturing Examples

The samples use example data for a memory foam bed factory:

| Item | Description |
|------|-------------|
| 20821 | Carabin Puller-Blue |
| 20697 | Feel Label Soft-Blue |

**Replace these with your actual item numbers!**

## 🛠️ Development Tips

### Use appsettings.Development.json

Create this file (git-ignored) for local testing:

```json
{
  "AzureAd": {
    "ClientSecret": "your-dev-secret"
  }
}
```

### Enable Debug Logging

In `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### Test Authentication Separately

Run just the token acquisition:

```csharp
var token = await tokenProvider.GetTokenAsync();
Console.WriteLine($"Token acquired: {token.Substring(0, 20)}...");
```

## 📞 Support Resources

- **Setup Issues**: Review [SETUP.md](./SETUP.md)
- **Permissions**: See [PERMISSIONS.md](./PERMISSIONS.md)
- **API Details**: Check individual sample README files
- **Microsoft Docs**: Links in main [README.md](./README.md)

## ✅ Checklist

Before going to production:

- [ ] Tested all samples in development environment
- [ ] Verified authentication works reliably
- [ ] Customized sample code for your item numbers
- [ ] Implemented error handling and retry logic
- [ ] Set up proper logging/monitoring
- [ ] Reviewed security best practices
- [ ] Documented your integration for operations team
- [ ] Tested failover scenarios
- [ ] Created runbooks for troubleshooting

---

**Ready to integrate?** Start with the Inventory Visibility sample, then move to MES Integration and OData queries. Good luck! 🚀
