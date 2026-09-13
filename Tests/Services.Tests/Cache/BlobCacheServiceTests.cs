using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Azure;
using Moq;
using ChrisUsher.Core.Services.Cache;

namespace Services.Tests.Cache;

[TestFixture]
public class BlobCacheServiceTests
{
    private BlobCacheService _cacheService;
    private ILogger<BlobCacheService> _logger;

    [SetUp]
    public void SetUp()
    {
        _logger = new Mock<ILogger<BlobCacheService>>().Object;

        // Cache uses Storage account
        var blobClientFactory = ServiceTestsCommon.Services.GetRequiredService<IAzureClientFactory<BlobServiceClient>>();
        var blobServiceClient = blobClientFactory.CreateClient("Storage");
        _cacheService = new BlobCacheService(blobServiceClient, _logger);
    }

    [Test]
    public async Task GetAsync_CacheMiss_ReturnsNull()
    {
        // Arrange
        var cacheKey = "test/miss-key.json";

        // Act
        var result = await _cacheService.GetAsync<User>(cacheKey);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SetAsync_ThenGetAsync_WithinTTL_ReturnsCachedData()
    {
        // Arrange
        var cacheKey = "test/user-cache-test.json";
        var testUser = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            EmailAddress = "test@example.com",
        };
        var ttl = TimeSpan.FromMinutes(10);

        // Act - Set cache
        await _cacheService.SetAsync(cacheKey, testUser, ttl);

        // Act - Get from cache
        var result = await _cacheService.GetAsync<User>(cacheKey);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(testUser.UserId));
        Assert.That(result.FirstName, Is.EqualTo(testUser.FirstName));
        Assert.That(result.LastName, Is.EqualTo(testUser.LastName));
        Assert.That(result.EmailAddress, Is.EqualTo(testUser.EmailAddress));
    }

    [Test]
    public async Task GetAsync_ExpiredTTL_ReturnsNull()
    {
        // Arrange
        var cacheKey = "test/expired-cache-test.json";
        var testUser = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = "Expired",
            LastName = "User",
            EmailAddress = "expired@example.com"
        };
        var ttl = TimeSpan.FromMilliseconds(1); // Very short TTL

        // Act - Set cache
        await _cacheService.SetAsync(cacheKey, testUser, ttl);

        // Wait for expiration
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        // Act - Try to get from cache after expiration
        var result = await _cacheService.GetAsync<User>(cacheKey);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ExistsAsync_ValidCache_ReturnsTrue()
    {
        // Arrange
        var cacheKey = "test/exists-test.json";
        var testUser = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = "Exists",
            LastName = "User"
        };
        var ttl = TimeSpan.FromMinutes(5);

        // Act - Set cache
        await _cacheService.SetAsync(cacheKey, testUser, ttl);

        // Act - Check existence
        var exists = await _cacheService.ExistsAsync(cacheKey);

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsAsync_ExpiredCache_ReturnsFalse()
    {
        // Arrange
        var cacheKey = "test/exists-expired-test.json";
        var testUser = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = "ExistsExpired",
            LastName = "User"
        };
        var ttl = TimeSpan.FromMilliseconds(1);

        // Act - Set cache
        await _cacheService.SetAsync(cacheKey, testUser, ttl);

        // Wait for expiration
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        // Act - Check existence after expiration
        var exists = await _cacheService.ExistsAsync(cacheKey);

        // Assert
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task RemoveAsync_ExistingCache_RemovesSuccessfully()
    {
        // Arrange
        var cacheKey = "test/remove-test.json";
        var testUser = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = "Remove",
            LastName = "User"
        };
        var ttl = TimeSpan.FromMinutes(5);

        // Act - Set cache
        await _cacheService.SetAsync(cacheKey, testUser, ttl);

        // Verify it exists
        var existsBefore = await _cacheService.ExistsAsync(cacheKey);
        Assert.That(existsBefore, Is.True);

        // Act - Remove cache
        await _cacheService.RemoveAsync(cacheKey);

        // Act - Check existence after removal
        var existsAfter = await _cacheService.ExistsAsync(cacheKey);

        // Assert
        Assert.That(existsAfter, Is.False);
    }

    [Test]
    public async Task SetAsync_OverwriteExisting_UpdatesCache()
    {
        // Arrange
        var cacheKey = "test/overwrite-test.json";
        var originalUser = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = "Original",
            LastName = "User"
        };
        var updatedUser = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = "Updated",
            LastName = "User"
        };
        var ttl = TimeSpan.FromMinutes(5);

        // Act - Set original cache
        await _cacheService.SetAsync(cacheKey, originalUser, ttl);

        // Act - Overwrite with updated cache
        await _cacheService.SetAsync(cacheKey, updatedUser, ttl);

        // Act - Get from cache
        var result = await _cacheService.GetAsync<User>(cacheKey);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FirstName, Is.EqualTo("Updated"));
    }

    [TearDown]
    public async Task TearDown()
    {
        // Clean up test cache entries
        var testKeys = new[]
        {
            "test/miss-key.json",
            "test/user-cache-test.json",
            "test/expired-cache-test.json",
            "test/exists-test.json",
            "test/exists-expired-test.json",
            "test/remove-test.json",
            "test/overwrite-test.json"
        };

        foreach (var key in testKeys)
        {
            try
            {
                await _cacheService.RemoveAsync(key);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private class User
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
    }
}