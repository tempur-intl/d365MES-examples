namespace D365.Auth.Models;

/// <summary>
/// Configuration settings for Azure AD authentication
/// </summary>
public class AzureAdConfig
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Scope { get; set; } = "https://inventoryservice.operations365.dynamics.com/.default";
}

/// <summary>
/// Configuration settings for D365 environment
/// </summary>
public class D365Config
{
    public string EnvironmentId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
}

/// <summary>
/// Configuration settings for Inventory Visibility Add-in
/// </summary>
public class IvaConfig
{
    public string SecurityServiceUrl { get; set; } = "https://securityservice.operations365.dynamics.com";
    public string ServiceUrl { get; set; } = "https://inventoryservice.operations365.dynamics.com";
}

/// <summary>
/// Represents an access token with expiration tracking
/// </summary>
public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public DateTime ExpiresAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt.AddMinutes(-5); // 5-minute buffer
}
