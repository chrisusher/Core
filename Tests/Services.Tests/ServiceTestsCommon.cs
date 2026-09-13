using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Serilog;
using ChrisUsher.Core.Shared.Tests;

namespace Services.Tests;

public static class ServiceTestsCommon
{
    private static ServiceProvider? _services;

    public static TestConfig? Config { get; internal set; }

    public static IConfigurationRoot? Configuration { get; internal set; }

    public static ServiceProvider Services
    {
        get
        {
            if (_services == null)
            {
                _services = RegisterServices();
            }
            return _services;
        }
    }

    private static ServiceProvider RegisterServices()
    {
        var services = new ServiceCollection();

        // Configure Serilog from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        Configuration = configuration;

        // Create Serilog logger
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        // Add Serilog to the DI container
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(Log.Logger);
        });
        
        // Test-only: register a default (unnamed) BlobServiceClient so services that inject
        // BlobServiceClient (without a name) resolve successfully (e.g., ExchangeDataService)
        services.AddAzureClients(config =>
        {
            var cs = Configuration!.GetConnectionString("Storage")
                     ?? "UseDevelopmentStorage=true";
            config.AddBlobServiceClient(cs);
        });
        
        services.AddSingleton<IConfiguration>(Configuration!);

        var serviceCollection = services.BuildServiceProvider();

        return serviceCollection;
    }
}