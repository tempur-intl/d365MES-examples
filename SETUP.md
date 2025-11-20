# Setup Guide for D365 Integration Samples

This guide walks through setting up Azure AD authentication and configuring your D365 environment for the integration samples.

## 📋 Prerequisites

- [ ] **D365 Supply Chain Management** environment (cloud deployment)
- [ ] **Azure subscription** (for app registration)
- [ ] **D365 admin access** (to grant permissions)
- [ ] **.NET 8.0 SDK** installed
- [ ] **Visual Studio Code** or **Visual Studio 2022**

## 🔐 Part 1: Azure AD App Registration

### Step 1: Create App Registration

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** → **App registrations**
3. Click **New registration**
4. Enter details:
   - **Name**: `D365-MES-Integration`
   - **Supported account types**: Single tenant
   - **Redirect URI**: Leave blank (not needed for service-to-service)
5. Click **Register**

### Step 2: Note Application IDs

After registration, copy these values (you'll need them later):

- **Application (client) ID** → Save as `ClientId`
- **Directory (tenant) ID** → Save as `TenantId`

### Step 3: Create Client Secret

1. In your app registration, go to **Certificates & secrets**
2. Click **New client secret**
3. Enter description: `MES Integration Secret`
4. Select expiration: Choose based on your security policy
5. Click **Add**
6. **IMPORTANT**: Copy the secret **Value** immediately → Save as `ClientSecret`
   - You won't be able to see it again!

### Step 4: Grant API Permissions

#### For D365 and MES Integration:

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Dynamics 365** (or search for "Dynamics ERP")
4. Choose **Delegated permissions** or **Application permissions**
5. Select **Dynamics.ERP → user_impersonation**
6. Click **Add permissions**
7. Click **Grant admin consent for [Your Organization]**

#### For Inventory Visibility Add-in:

1. Click **Add a permission** again
2. Select **APIs my organization uses**
3. Search for: `0cdb527f-a8d1-4bf8-9436-b352c68682b2`
4. Select the Inventory Visibility App
5. Choose **Application permissions**
6. Click **Add permissions**
7. Click **Grant admin consent**

## 🏢 Part 2: D365 Environment Configuration

### Step 1: Get Environment ID

1. Go to [Lifecycle Services (LCS)](https://lcs.dynamics.com)
2. Open your D365 project
3. Click on your environment
4. Copy the **Environment ID** (format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`)

### Step 2: Get D365 Base URL

Your D365 base URL format:
```
https://[yourinstance].operations.dynamics.com
```

Example: `https://contoso-prod.operations.dynamics.com`

### Step 3: Enable Time and Attendance License (for MES)

1. In D365, go to **System administration** → **Setup** → **License configuration**
2. Scroll to **Time and attendance**
3. Check the box to enable it
4. **Note**: You need to enable maintenance mode first:
   - System administration → Maintenance mode
   - Turn ON maintenance mode
   - Enable the license key
   - Turn OFF maintenance mode

### Step 4: Set Up Azure AD App in D365

1. In D365, go to **System administration** → **Setup** → **Azure Active Directory applications**
2. Click **New**
3. Enter:
   - **Client ID**: Your Azure AD app's Client ID
   - **Name**: `MES Integration`
   - **User ID**: Select a system user account
4. Click **Save**

### Step 5: Assign Security Role

The Azure AD app user needs appropriate permissions:

1. Go to **System administration** → **Security** → **Assign users to roles**
2. Select the system user you assigned in Step 4
3. Assign roles based on what you need:
   - For MES: **Manufacturing execution user**
   - For Inventory: **Inventory visibility integration user**
   - For OData: **System user** (or custom role with read access)

## 📦 Part 3: Install Inventory Visibility Add-in

### Step 1: Enable in LCS

1. Go to [LCS](https://lcs.dynamics.com) → Your project
2. Click **Environment features**
3. Find **Inventory Visibility**
4. Click **Install**
5. Wait for installation to complete (can take 30-60 minutes)

### Step 2: Configure in Power Platform

1. Go to [Power Platform Admin Center](https://admin.powerplatform.microsoft.com/)
2. Find your environment linked to D365
3. Open **Inventory Visibility**
4. Configure:
   - Data sources
   - Partition strategy
   - Index hierarchy
   - Soft reservation configuration

Detailed guide: [Inventory Visibility Setup](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-visibility-setup)

## ⚙️ Part 4: Configure Sample Projects

### Step 1: Clone Repository

```bash
git clone <your-repo-url>
cd D365IntegrationSamples
```

### Step 2: Update Configuration Files

Each sample project has an `appsettings.json` file. Update with your values:

```json
{
  "AzureAd": {
    "TenantId": "YOUR-TENANT-ID",
    "ClientId": "YOUR-CLIENT-ID",
    "ClientSecret": "YOUR-CLIENT-SECRET"
  },
  "D365": {
    "EnvironmentId": "YOUR-ENVIRONMENT-ID",
    "BaseUrl": "https://YOUR-INSTANCE.operations.dynamics.com",
    "OrganizationId": "YOUR-LEGAL-ENTITY"
  },
  "InventoryVisibility": {
    "SecurityServiceUrl": "https://securityservice.operations365.dynamics.com",
    "ServiceUrl": "https://inventoryservice.operations365.dynamics.com"
  }
}
```

### Configuration Values Checklist

- [ ] **TenantId**: From Azure AD (Step 1.2)
- [ ] **ClientId**: From Azure AD (Step 1.2)
- [ ] **ClientSecret**: From Azure AD (Step 1.3)
- [ ] **EnvironmentId**: From LCS (Step 2.1)
- [ ] **BaseUrl**: Your D365 URL (Step 2.2)
- [ ] **OrganizationId**: Your legal entity (e.g., "usmf")

### Step 3: Build Solution

```bash
dotnet build
```

### Step 4: Test Authentication

Run a simple sample to verify authentication works:

```bash
cd samples/InventoryVisibility.Samples
dotnet run
```

If authentication works, you should see log messages about acquiring tokens.

## 🧪 Part 5: Testing the Samples

### Test 1: Inventory Visibility

```bash
cd samples/InventoryVisibility.Samples
dotnet run
```

**Expected Output**:
- Token acquisition messages
- Query results (may be empty if no data)
- On-hand change confirmation
- Reservation confirmation

### Test 2: MES Integration

Update `Program.cs` with a valid production order number from your D365:

```csharp
var productionOrderNumber = "YOUR-PROD-ORDER"; // Change this
```

Then run:

```bash
cd samples/MesIntegration.Samples
dotnet run
```

**Expected Output**:
- Token acquisition messages
- Message sent confirmations for each operation

**Verify in D365**:
- Go to **Production control** → **Setup** → **Manufacturing execution** → **Manufacturing execution systems integration**
- Look for your messages with status "Success"

### Test 3: OData Queries

Update entity names/IDs to match your D365 data:

```csharp
var product = await odataService.GetProductAsync("YOUR-ITEM-NUMBER");
```

Then run:

```bash
cd samples/OData.Samples
dotnet run
```

**Expected Output**:
- Token acquisition messages
- Query results with your D365 data

## 🔍 Part 6: Troubleshooting

### Problem: "AADSTS700016: Application not found"

**Solution**:
- Verify Client ID is correct
- Check app registration exists in Azure AD
- Ensure you're using the right tenant

### Problem: "AADSTS7000215: Invalid client secret"

**Solution**:
- Client secret may have expired
- Generate new secret in Azure AD
- Update appsettings.json

### Problem: "401 Unauthorized" calling D365 APIs

**Solution**:
- Check Azure AD app is registered in D365 (Part 2, Step 4)
- Verify security role assignment (Part 2, Step 5)
- Ensure API permissions granted with admin consent

### Problem: "Failed to acquire IVA token"

**Solution**:
- Verify Inventory Visibility add-in is installed
- Check environment ID is correct
- Ensure IVA API permissions are granted

### Problem: MES messages stuck in "Processing"

**Solution**:
- Check batch job is running: System administration → Batch jobs
- Look for "Manufacturing execution systems integration"
- Verify Time and attendance license is enabled

### Problem: OData query returns "Entity not found"

**Solution**:
- Entity names are case-sensitive
- Not all entities are exposed via OData
- Check $metadata endpoint for available entities

## 📊 Part 7: Monitoring and Logging

### Enable Detailed Logging

Update `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",  // Change from Information to Debug
      "Microsoft": "Warning"
    }
  }
}
```

### View MES Message Status

In D365:
1. **Production control** → **Setup** → **Manufacturing execution** → **Manufacturing execution systems integration**
2. Filter by:
   - Status: Failed (to see errors)
   - Date range: Today
3. Click on failed messages to see error details

### Monitor API Calls

Consider adding Application Insights:

```csharp
services.AddApplicationInsightsTelemetry();
```

## 🔐 Part 8: Security Best Practices

1. **Secrets Management**:
   - Don't commit `appsettings.json` with real secrets to source control
   - Use Azure Key Vault in production
   - Use `appsettings.Development.json` for local development

2. **Least Privilege**:
   - Grant only necessary permissions
   - Use separate app registrations for different integrations

3. **Secret Rotation**:
   - Rotate client secrets every 6-12 months
   - Set calendar reminders before expiration

4. **Audit Logging**:
   - Enable Azure AD sign-in logs
   - Monitor API usage in D365

## 📚 Additional Resources

- [Azure AD App Registration](https://learn.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
- [D365 Authentication](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/data-entities/services-home-page)
- [Inventory Visibility Setup](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-visibility-setup)
- [MES Integration](https://learn.microsoft.com/en-us/dynamics365/supply-chain/production-control/mes-integration)

## 🆘 Getting Help

If you encounter issues:

1. Check error messages carefully - they usually indicate the problem
2. Verify all configuration values are correct
3. Test authentication separately from data operations
4. Review D365 and Azure AD audit logs
5. Consult Microsoft Learn documentation for specific APIs

## ✅ Setup Complete!

Once all samples run successfully, your MES vendor developers can use these as templates for their integration.
