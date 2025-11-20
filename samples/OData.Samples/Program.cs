using System.Text.Json;
using D365.Auth.Models;
using D365.Auth.Providers;
using OData.Samples.Models;
using OData.Samples.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OData.Samples;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Setup dependency injection
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
            })
            .AddHttpClient()
            .AddSingleton(configuration.GetSection("AzureAd").Get<AzureAdConfig>()!)
            .AddSingleton(configuration.GetSection("D365").Get<D365Config>()!)
            .AddScoped<AzureAdTokenProvider>()
            .AddScoped<D365TokenProvider>()
            .AddScoped<ODataService>()
            .BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("=== D365 OData Query Samples ===\n");

        try
        {
            // Load sample queries from JSON file
            var sampleQueriesPath = Path.Combine(Directory.GetCurrentDirectory(), "sample-queries.json");
            if (!File.Exists(sampleQueriesPath))
            {
                logger.LogError("Sample queries file not found: {Path}", sampleQueriesPath);
                logger.LogError("Please create sample-queries.json with your query parameters");
                return;
            }

            var sampleQueriesJson = await File.ReadAllTextAsync(sampleQueriesPath);
            var sampleQueries = JsonSerializer.Deserialize<SampleQueryConfig>(sampleQueriesJson);

            if (sampleQueries == null)
            {
                logger.LogError("Failed to parse sample-queries.json");
                return;
            }

            logger.LogInformation("Loaded sample queries from configuration file\n");

            var odataService = serviceProvider.GetRequiredService<ODataService>();

            // Run samples
            await RunProductionOrderQuerySample(odataService, sampleQueries, logger);
            await RunProductQuerySample(odataService, sampleQueries, logger);
            await RunBomQuerySample(odataService, sampleQueries, logger);
            await RunRouteQuerySample(odataService, sampleQueries, logger);

            logger.LogInformation("\n=== All OData query samples completed successfully ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running OData query samples");
        }
    }

    static async Task RunProductionOrderQuerySample(ODataService odataService, SampleQueryConfig sampleQueries, ILogger logger)
    {
        logger.LogInformation("\n--- Sample 1: Query Production Orders ---");

        var orders = await odataService.GetProductionOrdersAsync(
            filter: sampleQueries.ProductionOrders.Filter,
            top: sampleQueries.ProductionOrders.Top);

        var json = JsonSerializer.Serialize(orders, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    static async Task RunProductQuerySample(ODataService odataService, SampleQueryConfig sampleQueries, ILogger logger)
    {
        logger.LogInformation("\n--- Sample 2: Query Product Information ---");

        var product = await odataService.GetProductAsync(sampleQueries.Product.ProductNumber);

        var json = JsonSerializer.Serialize(product, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    static async Task RunBomQuerySample(ODataService odataService, SampleQueryConfig sampleQueries, ILogger logger)
    {
        logger.LogInformation("\n--- Sample 3: Query BOM Lines ---");

        var bomLines = await odataService.GetBomLinesAsync(sampleQueries.Bom.ProductionOrderNumber);

        var json = JsonSerializer.Serialize(bomLines, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    static async Task RunRouteQuerySample(ODataService odataService, SampleQueryConfig sampleQueries, ILogger logger)
    {
        logger.LogInformation("\n--- Sample 4: Query Route Operations ---");

        var operations = await odataService.GetRouteOperationsAsync(sampleQueries.Route.ProductionOrderNumber);

        var json = JsonSerializer.Serialize(operations, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }


}
