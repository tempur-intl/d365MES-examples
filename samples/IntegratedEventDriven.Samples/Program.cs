using D365.Auth.Models;
using D365.Auth.Providers;
using IntegratedEventDriven.Samples.Models;
using IntegratedEventDriven.Samples.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IntegratedEventDriven.Samples;

class Program
{
    static async Task Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        // Get configuration sections with validation
        var azureAdConfig = configuration.GetSection("AzureAd").Get<AzureAdConfig>();
        var d365Config = configuration.GetSection("D365").Get<D365Config>();
        var serviceBusConfig = configuration.GetSection("ServiceBus").Get<ServiceBusConfig>();

        if (azureAdConfig == null || d365Config == null || serviceBusConfig == null)
        {
            Console.WriteLine("ERROR: Missing required configuration sections.");
            Console.WriteLine("Please copy appsettings.Example.json to appsettings.Development.json and configure with your values.");
            Environment.Exit(1);
        }

        var serviceProvider = new ServiceCollection()
            .AddHttpClient()
            .AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
            })
            .AddSingleton(azureAdConfig)
            .AddSingleton(d365Config)
            .AddSingleton(serviceBusConfig)
            .AddScoped<AzureAdTokenProvider>()
            .AddScoped<D365TokenProvider>()
            .AddScoped<IntegratedService>()
            .BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("=== D365 Integrated Event-Driven Sample ===");
        logger.LogInformation("This sample demonstrates:");
        logger.LogInformation("1. Consuming TSIProductionOrderReleasedToMESBusinessEvent from Service Bus");
        logger.LogInformation("2. Using the ProductionOrderNumber to query OData for details");
        logger.LogInformation("3. Retrieving related BOM lines for the production order\n");

        // Setup cancellation for Ctrl+C
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            logger.LogInformation("\nShutting down gracefully...");
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            var integratedService = serviceProvider.GetRequiredService<IntegratedService>();

            // Parse command-line arguments
            var maxMessages = 10;
            if (args.Length > 0 && int.TryParse(args[0], out var parsedMax))
            {
                maxMessages = parsedMax;
            }

            await integratedService.ProcessEventsAsync(maxMessages, cts.Token);
            await integratedService.DisposeAsync();

            logger.LogInformation("\n=== Processing completed ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running integrated event-driven sample");
            Environment.Exit(1);
        }
    }
}
