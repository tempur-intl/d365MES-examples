using System.Text.Json.Serialization;

namespace D365.Auth.Models;

/// <summary>
/// Azure AD token response
/// </summary>
internal class AzureAdTokenResponse
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("ext_expires_in")]
    public int ExtExpiresIn { get; set; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
}

/// <summary>
/// IVA security service token request
/// </summary>
internal class IvaSecurityTokenRequest
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = "client_credentials";

    [JsonPropertyName("client_assertion_type")]
    public string ClientAssertionType { get; set; } = "aad_app";

    [JsonPropertyName("client_assertion")]
    public string ClientAssertion { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = "https://inventoryservice.operations365.dynamics.com/.default";

    [JsonPropertyName("context")]
    public string Context { get; set; } = string.Empty;

    [JsonPropertyName("context_type")]
    public string ContextType { get; set; } = "finops-env";
}

/// <summary>
/// IVA security service token response
/// </summary>
internal class IvaSecurityTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
