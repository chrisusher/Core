using System.Diagnostics;
using Azure.Storage.Blobs;
using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Azure;
using ChrisUsher.Core.Shared.Tests;

namespace Services.Tests;

[SetUpFixture]
public class AssemblyLifecycle
{
    private BlobServiceClient? _blobService;
    private static bool? _runningInActions;

    private static bool IsRunningInGitHubActions
    {
        get
        {
            if (_runningInActions == null)
            {
                _runningInActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
            }
            return _runningInActions.Value;
        }
    }

    [OneTimeSetUp]
    public async Task AssemblySetup()
    {
        var initialConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        ServiceTestsCommon.Config = initialConfig.GetSection("Tests").Get<TestConfig>();

        if (IsRunningInGitHubActions)
        {
            await SetupLocalEnvironmentAsync();

            DotEnv.Load(new DotEnvOptions(ignoreExceptions: true, envFilePaths: new[]
            {
                ".env"
            }));
        }

        ServiceTestsCommon.Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        await Task.CompletedTask;
    }

    private async Task SetupLocalEnvironmentAsync()
    {
        try
        {
            DotEnv.Load(new DotEnvOptions(ignoreExceptions: false, envFilePaths: new[] { ".env" }));
        }
        catch (Exception)
        {
            if (!IsRunningInGitHubActions)
            {
                throw;
            }
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "compose -f ./docker-compose.ci.yml up -d --remove-orphans",
            UseShellExecute = false,
            WorkingDirectory = Environment.CurrentDirectory
        });

        // Get the named "MarketData" BlobServiceClient for MarketData storage
        var blobClientFactory = ServiceTestsCommon.Services.GetRequiredService<IAzureClientFactory<BlobServiceClient>>();
        _blobService = blobClientFactory.CreateClient("Storage");

        await CreateBlobTestDataAsync();
    }

    private async Task CreateBlobTestDataAsync()
    {
        await CreateBlobContainersAsync();
    }

    private async Task CreateBlobContainersAsync()
    {
        // Create cache container for testing
        var containerClient = _blobService!.GetBlobContainerClient("cache");

        if (!await containerClient.ExistsAsync())
        {
            await containerClient.CreateIfNotExistsAsync();
        }
    }

    [OneTimeTearDown]
    public async Task AssemblyTearDownAsync()
    {
        if (ServiceTestsCommon.Config!.LocalSetup())
        {
            var stopProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "compose -f ./docker-compose.ci.yml down",
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory
            });

            if (stopProcess != null)
            {
                stopProcess.WaitForExit(30_000);
            }
        }
    }
}