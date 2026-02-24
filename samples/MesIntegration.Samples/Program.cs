using System.Text.Json;
using D365.Auth.Models;
using D365.Auth.Providers;
using MesIntegration.Samples.Models;
using MesIntegration.Samples.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MesIntegration.Samples;

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
            .AddScoped<MesService>()
            .BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("=== D365 MES Integration Samples ===\n");

        try
        {
            // Load sample data from JSON file
            var sampleDataPath = Path.Combine(Directory.GetCurrentDirectory(), "sample-data.json");
            if (!File.Exists(sampleDataPath))
            {
                logger.LogError("Sample data file not found: {Path}", sampleDataPath);
                logger.LogError("Please create sample-data.json with your production order data");
                return;
            }

            var sampleDataJson = await File.ReadAllTextAsync(sampleDataPath);
            var sampleData = JsonSerializer.Deserialize<SampleDataConfig>(sampleDataJson);

            if (sampleData == null)
            {
                logger.LogError("Failed to parse sample-data.json");
                return;
            }

            logger.LogInformation("Loaded sample data for production order: {OrderNumber}\n",
                sampleData.ProductionOrderNumber);

            var mesService = serviceProvider.GetRequiredService<MesService>();

            // Simulate a complete production order lifecycle using data from JSON
            await RunStartProductionOrderSample(mesService, sampleData, logger);
            await RunMaterialConsumptionSample(mesService, sampleData, logger);
            // We currently do not use time consumption reporting in our MES processes
            // await RunTimeConsumptionSample(mesService, sampleData, logger);
            await RunReportAsFinishedSample(mesService, sampleData, logger);
            await RunEndProductionOrderSample(mesService, sampleData, logger);

            logger.LogInformation("\n=== All MES integration samples completed successfully ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running MES integration samples");
        }
    }

    static async Task RunStartProductionOrderSample(
        MesService mesService,
        SampleDataConfig sampleData,
        ILogger logger)
    {
        logger.LogInformation("\n--- Sample 1: Start Production Order ---");

        var message = new StartProductionOrderMessage
        {
            ProductionOrderNumber = sampleData.ProductionOrderNumber,
            StartedQuantity = sampleData.StartedQuantity,
            StartedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            AutomaticBOMConsumptionRule = sampleData.AutomaticBOMConsumptionRule,
            AutomaticRouteConsumptionRule = sampleData.AutomaticRouteConsumptionRule
        };

        await mesService.StartProductionOrderAsync(message);
        logger.LogInformation("Production order {OrderNumber} started", sampleData.ProductionOrderNumber);
    }

    static async Task RunMaterialConsumptionSample(
        MesService mesService,
        SampleDataConfig sampleData,
        ILogger logger)
    {
        logger.LogInformation("\n--- Sample 2: Report Material Consumption ---");

        var message = new MaterialConsumptionMessage
        {
            ProductionOrderNumber = sampleData.ProductionOrderNumber,
            PickingListLines = sampleData.MaterialConsumption.Select(m => new PickingListLine
            {
                ItemNumber = m.ItemNumber,
                ConsumptionBOMQuantity = m.ConsumptionBOMQuantity,
                ConsumptionDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                OperationNumber = m.OperationNumber,
                ProductionSiteId = m.ProductionSiteId,
                ProductionWarehouseId = m.ProductionWarehouseId
            }).ToList()
        };

        await mesService.ReportMaterialConsumptionAsync(message);
        logger.LogInformation("Material consumption reported for {OrderNumber}", sampleData.ProductionOrderNumber);
    }

    // We currently do not use time consumption reporting in our MES processes
    // static async Task RunTimeConsumptionSample(
    //     MesService mesService,
    //     SampleDataConfig sampleData,
    //     ILogger logger)
    // {
    //     logger.LogInformation("\n--- Sample 3: Report Time Consumption ---");

    //     var message = new RouteCardMessage
    //     {
    //         ProductionOrderNumber = sampleData.ProductionOrderNumber,
    //         RouteCardLines = sampleData.TimeConsumption.Select(t => new RouteCardLine
    //         {
    //             OperationNumber = t.OperationNumber,
    //             Hours = t.Hours,
    //             GoodQuantity = t.GoodQuantity,
    //             ErrorQuantity = t.ErrorQuantity,
    //             ConsumptionDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
    //             OperationsResourceId = t.OperationsResourceId,
    //             Worker = t.Worker
    //         }).ToList()
    //     };

    //     await mesService.ReportTimeConsumptionAsync(message);
    //     logger.LogInformation("Time consumption reported for {OrderNumber}", sampleData.ProductionOrderNumber);
    // }

    static async Task RunReportAsFinishedSample(
        MesService mesService,
        SampleDataConfig sampleData,
        ILogger logger)
    {
        logger.LogInformation("\n--- Sample 4: Report As Finished ---");

        var message = new ReportAsFinishedMessage
        {
            ProductionOrderNumber = sampleData.ProductionOrderNumber,
            ReportFinishedLines = new List<ReportFinishedLine>
            {
                new()
                {
                    ItemNumber = sampleData.ReportAsFinished.ItemNumber,
                    ReportedGoodQuantity = sampleData.ReportAsFinished.ReportedGoodQuantity,
                    ReportedErrorQuantity = sampleData.ReportAsFinished.ReportedErrorQuantity,
                    ReportAsFinishedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    ProductionSiteId = sampleData.ReportAsFinished.ProductionSiteId,
                    ProductionWarehouseId = sampleData.ReportAsFinished.ProductionWarehouseId,
                    ProductionWarehouseLocationId = sampleData.ReportAsFinished.ProductionWarehouseLocationId,
                    ItemBatchNumber = $"BATCH-{DateTime.UtcNow:yyyyMMdd}",
                    AutomaticBOMConsumptionRule = sampleData.ReportAsFinished.AutomaticBOMConsumptionRule,
                    AutomaticRouteConsumptionRule = sampleData.ReportAsFinished.AutomaticRouteConsumptionRule,
                    EndJob = sampleData.ReportAsFinished.EndJob,
                    GenerateLicensePlate = sampleData.ReportAsFinished.GenerateLicensePlate
                }
            },
            PrintLabel = sampleData.ReportAsFinished.PrintLabel
        };

        await mesService.ReportAsFinishedAsync(message);
        logger.LogInformation("Report as finished completed for {OrderNumber}", sampleData.ProductionOrderNumber);
    }

    static async Task RunEndProductionOrderSample(
        MesService mesService,
        SampleDataConfig sampleData,
        ILogger logger)
    {
        logger.LogInformation("\n--- Sample 5: End Production Order ---");

        var message = new EndProductionOrderMessage
        {
            ProductionOrderNumber = sampleData.ProductionOrderNumber,
            EndedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            AutoUpdate = "Yes"
        };

        await mesService.EndProductionOrderAsync(message);
        logger.LogInformation("Production order {OrderNumber} ended", sampleData.ProductionOrderNumber);
    }
}
