using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Azure;
using ChrisUsher.Core.Shared.Tests;

namespace Services.Tests;

[SetUpFixture]
public class AssemblyLifecycle
{
    private BlobServiceClient? _blobService;
    private bool _localEnvironmentWasSetUp;

    [OneTimeSetUp]
    public async Task AssemblySetup()
    {
        var initialConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        ServiceTestsCommon.Config = initialConfig.GetSection("Tests").Get<TestConfig>()
            ?? new TestConfig();

        if (ServiceTestsCommon.Config.LocalSetup())
        {
            await SetupLocalEnvironmentAsync();
        }

        ServiceTestsCommon.Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        // The cache tests need this container regardless of whether Azurite was started
        // locally or by the CI setup above.
        var blobClientFactory = ServiceTestsCommon.Services
            .GetRequiredService<IAzureClientFactory<BlobServiceClient>>();
        _blobService = blobClientFactory.CreateClient("Storage");

        await CreateBlobTestDataAsync();
    }

    private async Task SetupLocalEnvironmentAsync()
    {
        var setupProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "compose -f ./docker-compose.ci.yml up -d",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            WorkingDirectory = Environment.CurrentDirectory
        }) ?? throw new InvalidOperationException("Unable to start Docker Compose for Azurite.");

        var standardError = setupProcess.StandardError.ReadToEndAsync();
        var standardOutput = setupProcess.StandardOutput.ReadToEndAsync();
        await setupProcess.WaitForExitAsync();

        if (setupProcess.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Unable to start the Azurite test dependency. {await standardError} {await standardOutput}");
        }

        _localEnvironmentWasSetUp = true;
        await WaitForAzuriteAsync();
    }

    private static async Task WaitForAzuriteAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(IPAddress.Loopback, 10000);
                return;
            }
            catch (SocketException)
            {
                await Task.Delay(250);
            }
        }

        throw new InvalidOperationException(
            "Azurite did not start listening on 127.0.0.1:10000 within 30 seconds.");
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
        if (_localEnvironmentWasSetUp)
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
