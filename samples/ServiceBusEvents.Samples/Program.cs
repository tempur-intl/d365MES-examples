using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBusEvents.Samples.Models;
using ServiceBusEvents.Samples.Services;

namespace ServiceBusEvents.Samples;

class Program
{
    static async Task Main(string[] args)
    {
        // Parse command line arguments
        var continuousMode = args.Contains("--continuous");
        var checkDlq = args.Contains("--check-dlq");
        var maxMessages = GetMaxMessagesArg(args);

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
            .AddSingleton(configuration.GetSection("ServiceBus").Get<ServiceBusConfig>()!)
            .AddScoped<ServiceBusConsumerService>()
            .BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("=== D365 Service Bus Event Consumer ===\n");

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
            var consumerService = serviceProvider.GetRequiredService<ServiceBusConsumerService>();

            if (checkDlq)
            {
                await RunDeadLetterQueueCheckAsync(consumerService, maxMessages, logger);
            }
            else if (continuousMode)
            {
                await RunContinuousModeAsync(consumerService, cts.Token, logger);
            }
            else
            {
                await RunPollOnceAsync(consumerService, maxMessages, logger);
            }

            await consumerService.DisposeAsync();
            logger.LogInformation("\n=== Service Bus consumer completed ===");
        }
        catch (Azure.Messaging.ServiceBus.ServiceBusException sbEx) when (sbEx.Reason == Azure.Messaging.ServiceBus.ServiceBusFailureReason.ServiceCommunicationProblem)
        {
            logger.LogError(sbEx, "Service Bus connection error - check network connectivity and firewall settings");
            logger.LogError("Troubleshooting steps:");
            logger.LogError("1. Verify Service Bus namespace networking settings in Azure Portal");
            logger.LogError("2. Check if 'Public network access' is enabled or if you need VPN");
            logger.LogError("3. Ensure firewall allows outbound TCP on port 5671 (AMQP)");
            logger.LogError("4. Verify connection string has 'Listen' permissions");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running Service Bus consumer");
            Environment.Exit(1);
        }
    }

    static async Task RunPollOnceAsync(
        ServiceBusConsumerService consumerService,
        int maxMessages,
        ILogger logger)
    {
        logger.LogInformation("--- Poll Once Mode ---");
        logger.LogInformation("Receiving up to {MaxMessages} messages...\n", maxMessages);

        var results = await consumerService.ReceiveMessagesAsync(maxMessages);

        if (results.Count == 0)
        {
            logger.LogInformation("No messages available");
            return;
        }

        logger.LogInformation("\nProcessed {Count} messages:", results.Count);

        foreach (var result in results)
        {
            var resultJson = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            Console.WriteLine(resultJson);
        }

        var successCount = results.Count(r => r.Success);
        var failureCount = results.Count(r => !r.Success);
        logger.LogInformation("\nSummary: {Success} succeeded, {Failed} failed",
            successCount, failureCount);
    }

    static async Task RunContinuousModeAsync(
        ServiceBusConsumerService consumerService,
        CancellationToken cancellationToken,
        ILogger logger)
    {
        logger.LogInformation("--- Continuous Listening Mode ---");
        logger.LogInformation("Press Ctrl+C to stop\n");

        await consumerService.StartContinuousProcessingAsync(cancellationToken);
    }

    static async Task RunDeadLetterQueueCheckAsync(
        ServiceBusConsumerService consumerService,
        int maxMessages,
        ILogger logger)
    {
        logger.LogInformation("--- Dead Letter Queue Inspection ---");
        logger.LogInformation("Checking for failed messages...\n");

        var results = await consumerService.CheckDeadLetterQueueAsync(maxMessages);

        if (results.Count == 0)
        {
            logger.LogInformation("No messages in dead letter queue");
            return;
        }

        logger.LogInformation("\nFound {Count} messages in DLQ:", results.Count);

        foreach (var result in results)
        {
            var resultJson = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            Console.WriteLine(resultJson);
        }
    }

    static int GetMaxMessagesArg(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--max-messages" && int.TryParse(args[i + 1], out int max))
            {
                return max;
            }
        }
        return 10; // Default
    }
}
