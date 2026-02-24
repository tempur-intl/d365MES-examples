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
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
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
            await RunTsiItemQuerySample(odataService, sampleQueries, logger);
            await RunTsiProdBomLinesQuerySample(odataService, sampleQueries, logger);
            await RunTsiLabelQuerySample(odataService, sampleQueries, logger);
            await RunTsiJobQuerySample(odataService, sampleQueries, logger);
            await RunWarehouseWorkLinesQuerySample(odataService, sampleQueries, logger);
            await RunItemBatchesQuerySample(odataService, sampleQueries, logger);

            logger.LogInformation("\n=== All OData query samples completed successfully ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running OData query samples");
        }
    }

    static async Task RunTsiItemQuerySample(ODataService odataService, SampleQueryConfig sampleQueries, ILogger logger)
    {
        logger.LogInformation("\n--- Sample 1: Query TSI Items ---");

        var items = await odataService.GetTsiItemsAsync(sampleQueries.TsiItems.ItemId);

        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    static async Task RunTsiProdBomLinesQuerySample(ODataService odataService, SampleQueryConfig sampleQueries, ILogger logger)
    {
        logger.LogInformation("\n--- Sample 2: Query TSI Production BOM Lines ---");

        var bomLines = await odataService.GetTsiProdBomLinesAsync(sampleQueries.TsiProdBomLines.ProdId);

        var json = JsonSerializer.Serialize(bomLines, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    static async Task RunTsiLabelQuerySample(ODataService odataService, SampleQueryConfig sampleQueries, ILogger logger)
    {
        logger.LogInformation("\n--- Sample 3: Query TSI Labels ---");

        var labels = await odataService.GetTsiLabelsAsync(
            prodId: sampleQueries.TsiLabels.ProdId,
            udiUnit: sampleQueries.TsiLabels.UDIUnit);

        var json = JsonSerializer.Serialize(labels, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    static async Task RunTsiJobQuerySample(ODataService odataService, SampleQueryConfig sampleQueries, ILogger logger)
    {
        logger.LogInformation("\n--- Sample 4: Query TSI Jobs ---");

        var jobs = await odataService.GetTsiJobsAsync(sampleQueries.TsiJobs.ProdId);

        var json = JsonSerializer.Serialize(jobs, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    static async Task RunWarehouseWorkLinesQuerySample(ODataService odataService, SampleQueryConfig sampleQueries, ILogger logger)
    {
        logger.LogInformation("\n--- Sample 5: Query Warehouse Work Lines ---");

        var workLines = await odataService.GetWarehouseWorkLinesAsync(
            filter: sampleQueries.WarehouseWorkLines.Filter,
            top: sampleQueries.WarehouseWorkLines.Top);

        var json = JsonSerializer.Serialize(workLines, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    static async Task RunItemBatchesQuerySample(ODataService odataService, SampleQueryConfig sampleQueries, ILogger logger)
    {
        logger.LogInformation("\n--- Sample 6: Query Item Batches ---");

        var batches = await odataService.GetItemBatchesAsync(
            filter: sampleQueries.ItemBatches.Filter,
            top: sampleQueries.ItemBatches.Top);

        var json = JsonSerializer.Serialize(batches, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

}
