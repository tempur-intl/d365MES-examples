using System.Net.Http.Headers;
using System.Net.Http.Json;
using D365.Auth.Models;
using D365.Auth.Providers;
using MesIntegration.Samples.Models;
using Microsoft.Extensions.Logging;

namespace MesIntegration.Samples.Services;

/// <summary>
/// Service for creating warehouse movement work via the TSI MES standalone service.
/// Unlike <see cref="MesService"/>, this calls D365 synchronously so callers receive
/// immediate confirmation or error feedback — no message queue involved.
/// </summary>
public class MovementWorkService
{
    private readonly HttpClient _httpClient;
    private readonly D365TokenProvider _tokenProvider;
    private readonly D365Config _config;
    private readonly ILogger<MovementWorkService> _logger;
    private const string Endpoint = "/api/services/TSIMesWebServices/TSIMesWebService/process";

    public MovementWorkService(
        HttpClient httpClient,
        D365TokenProvider tokenProvider,
        D365Config config,
        ILogger<MovementWorkService> logger)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Creates warehouse movement work for the given license plate.
    /// D365 processes the request synchronously and returns "Created" on success,
    /// or an error response that is surfaced immediately as an exception.
    /// </summary>
    /// <param name="contract">Movement work details. Only <c>LicensePlate</c> is required.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response string from D365 (e.g. <c>"Created"</c>).</returns>
    public async Task<string> CreateMovementWorkAsync(
        MovementWorkContract contract,
        CancellationToken cancellationToken = default)
    {
        // Default DataAreaId from config if not explicitly set
        if (string.IsNullOrEmpty(contract.DataAreaId))
        {
            contract.DataAreaId = _config.OrganizationId;
        }

        var request = new MovementWorkRequest { Contract = contract };

        _logger.LogInformation(
            "Creating movement work for license plate: {LicensePlate}",
            contract.LicensePlate);

        var token = await _tokenProvider.GetD365TokenAsync(cancellationToken);
        var url = $"{_config.BaseUrl}{Endpoint}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Failed to create movement work for license plate {LicensePlate}. Status: {StatusCode}, Error: {Error}",
                contract.LicensePlate,
                response.StatusCode,
                errorContent);
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation(
            "Movement work created successfully for license plate {LicensePlate}. Response: {Result}",
            contract.LicensePlate,
            result);

        return result;
    }
}
