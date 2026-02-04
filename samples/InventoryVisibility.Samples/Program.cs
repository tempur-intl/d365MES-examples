using System.Text.Json;
using D365.Auth.Models;
using D365.Auth.Providers;
using InventoryVisibility.Samples.Models;
using InventoryVisibility.Samples.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InventoryVisibility.Samples;

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
            .AddSingleton(configuration.GetSection("InventoryVisibility").Get<IvaConfig>()!)
            .AddScoped<AzureAdTokenProvider>()
            .AddScoped<IvaTokenProvider>()
            .AddScoped<IvaService>()
            .BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        Console.WriteLine("=== Inventory Visibility Query Sample ===\n");

        try
        {
            var ivaService = serviceProvider.GetRequiredService<IvaService>();
            var config = serviceProvider.GetRequiredService<D365Config>();

            // Query on-hand inventory
            var query = new OnHandQueryRequest
            {
                DimensionDataSource = "fno",
                Filters = new QueryFilters
                {
                    OrganizationId = new List<string> { config.OrganizationId },
                    ProductId = new List<string> { "85376" }, // Example product from your environment
                    SiteId = new List<string> { "01" },
                    LocationId = new List<string> { "010" }
                },
                GroupByValues = new List<string> { "BatchId", "LocationId", "WMSLocationId", "LicensePlateId" },
                ReturnNegative = true
            };

            var results = await ivaService.QueryOnHandAsync(query);

            // Output results as formatted JSON
            Console.WriteLine("\n=== Query Results ===");
            Console.WriteLine($"Found {results.Count} inventory record(s)\n");

            if (results.Count > 0)
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(results, jsonOptions);
                Console.WriteLine(json);
            }
            else
            {
                Console.WriteLine("No inventory records found for the specified criteria.");
            }

            Console.WriteLine("\n=== Query completed successfully ===");

            // Query on-hand inventory by LicensePlateId
            var licensePlateQuery = new OnHandQueryRequest
            {
                DimensionDataSource = "fno",
                Filters = new QueryFilters
                {
                    OrganizationId = new List<string> { config.OrganizationId },
                    LicensePlateId = new List<string> { "49001" } // Example LicensePlateId - replace with actual value
                },
                GroupByValues = new List<string> { "BatchId", "LocationId", "WMSLocationId", "SiteId" },
                ReturnNegative = true
            };

            var licensePlateResults = await ivaService.QueryOnHandAsync(licensePlateQuery);

            // Output results as formatted JSON
            Console.WriteLine("\n=== LicensePlateId Query Results ===");
            Console.WriteLine($"Found {licensePlateResults.Count} inventory record(s) for LicensePlateId\n");

            if (licensePlateResults.Count > 0)
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(licensePlateResults, jsonOptions);
                Console.WriteLine(json);
            }
            else
            {
                Console.WriteLine("No inventory records found for the specified LicensePlateId.");
            }

            Console.WriteLine("\n=== LicensePlateId query completed successfully ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running query");
        }
    }
}
