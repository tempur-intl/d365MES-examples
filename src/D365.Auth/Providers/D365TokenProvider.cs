using D365.Auth.Models;
using Microsoft.Extensions.Logging;

namespace D365.Auth.Providers;

/// <summary>
/// Provides authentication tokens for standard D365 APIs (OData, Message Service)
/// </summary>
public class D365TokenProvider
{
    private readonly AzureAdTokenProvider _azureAdTokenProvider;
    private readonly D365Config _config;
    private readonly ILogger<D365TokenProvider> _logger;

    public D365TokenProvider(
        AzureAdTokenProvider azureAdTokenProvider,
        D365Config config,
        ILogger<D365TokenProvider> logger)
    {
        _azureAdTokenProvider = azureAdTokenProvider ?? throw new ArgumentNullException(nameof(azureAdTokenProvider));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateConfiguration();
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_config.BaseUrl))
            throw new ArgumentException("BaseUrl is required", nameof(_config.BaseUrl));
    }

    /// <summary>
    /// Gets a D365 access token for OData and Message Service APIs
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token</returns>
    public async Task<string> GetD365TokenAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Acquiring D365 token");

        // For D365 Finance & Operations apps, the scope is the base URL + /.default
        var baseUri = new Uri(_config.BaseUrl);
        var scope = $"{baseUri.Scheme}://{baseUri.Host}/.default";

        var token = await _azureAdTokenProvider.GetTokenAsync(scope, cancellationToken);

        _logger.LogDebug("Successfully acquired D365 token");
        return token;
    }
}
