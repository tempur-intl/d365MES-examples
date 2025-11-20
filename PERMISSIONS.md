# Azure AD App Registration Permissions

This document lists all required Azure AD API permissions for the D365 integration samples.

## Required API Permissions

### 1. Dynamics 365 Finance and Operations

**Permission**: `user_impersonation`
**Type**: Delegated (or Application for service accounts)
**Required for**:
- MES Integration (Message Service)
- OData queries

**To add**:
1. Azure Portal → App registrations → Your app
2. API permissions → Add a permission
3. Select "APIs my organization uses"
4. Search for "Dynamics" or find "Dynamics 365"
5. Select "Dynamics ERP"
6. Choose permission type (Delegated or Application)
7. Check `user_impersonation`
8. Click "Add permissions"

### 2. Inventory Visibility Service

**Permission**: `Inventory Visibility App`
**Application ID**: `0cdb527f-a8d1-4bf8-9436-b352c68682b2`
**Type**: Application
**Required for**:
- Inventory Visibility Add-in queries
- IVA on-hand operations
- IVA reservations

**To add**:
1. Azure Portal → App registrations → Your app
2. API permissions → Add a permission
3. Select "APIs my organization uses"
4. Search for: `0cdb527f-a8d1-4bf8-9436-b352c68682b2`
5. Select the Inventory Visibility App
6. Choose "Application permissions"
7. Select available permissions
8. Click "Add permissions"

## Granting Admin Consent

After adding permissions, you **must** grant admin consent:

1. In API permissions page
2. Click **"Grant admin consent for [Your Organization]"**
3. Confirm the action
4. Verify status shows green checkmarks

**Without admin consent, authentication will fail!**

## Verifying Permissions

### Method 1: Azure Portal
1. Go to App registrations → Your app → API permissions
2. Check that all permissions show green checkmarks under "Status"

### Method 2: Token Inspection
Decode your access token at [jwt.ms](https://jwt.ms) and verify:
- `aud` (audience) matches your D365 URL or IVA service
- `scp` or `roles` contains expected permissions

## Permission Scopes

### For D365 APIs (OData, Message Service)

**Token scope format**:
```
https://[your-instance].operations.dynamics.com/.default
```

Example:
```
https://contoso-prod.operations.dynamics.com/.default
```

This is automatically constructed by the `D365TokenProvider`.

### For Inventory Visibility Add-in

**Azure AD token scope**:
```
0cdb527f-a8d1-4bf8-9436-b352c68682b2/.default
```

**IVA token scope**:
```
https://inventoryservice.operations365.dynamics.com/.default
```

Both are handled automatically by the `IvaTokenProvider`.

## Troubleshooting Permissions

### Error: "AADSTS65001: The user or administrator has not consented"

**Solution**: Grant admin consent (see above)

### Error: "AADSTS700016: Application not found in directory"

**Solution**:
- Check Client ID is correct
- Verify you're using the right Azure AD tenant

### Error: "Insufficient privileges to complete the operation"

**Solution**:
- Check D365 security role assignment
- Verify Azure AD app is registered in D365
- Ensure user/app has read/write permissions for entities

## Security Best Practices

1. **Principle of Least Privilege**: Only grant permissions actually needed
2. **Separate Apps**: Consider separate app registrations for different integrations
3. **Regular Audits**: Review permissions quarterly
4. **Monitor Usage**: Enable Azure AD sign-in logs and review regularly

## Permission Requirements by Sample

| Sample | Required Permission |
|--------|---------------------|
| Inventory Visibility | Inventory Visibility App (Application) |
| MES Integration | Dynamics 365 ERP - user_impersonation |
| OData Queries | Dynamics 365 ERP - user_impersonation |

## Additional Configuration

Besides Azure AD permissions, you also need:

1. **D365 Security Roles**:
   - Register Azure AD app in D365
   - Assign appropriate security role to app user

2. **License Keys** (for MES):
   - Enable "Time and attendance" license key in D365

3. **Add-in Installation** (for IVA):
   - Install Inventory Visibility add-in via LCS
   - Configure in Power Platform

See [SETUP.md](./SETUP.md) for complete setup instructions.
