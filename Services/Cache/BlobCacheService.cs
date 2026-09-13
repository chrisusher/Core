using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ChrisUsher.Core.Services.Interfaces;
using ChrisUsher.Core.Shared;
using Microsoft.Extensions.Logging;

namespace ChrisUsher.Core.Services.Cache;

/// <summary>
/// TTL-based cache implementation using Azure Blob Storage
/// Uses blob metadata to store expiration times and provides cache functionality with TTL
/// </summary>
public class BlobCacheService : ICacheService
{
    private const string EXPIRY_METADATA_KEY = "ExpiresAt";
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobCacheService> _logger;

    public BlobCacheService(BlobServiceClient blobServiceClient, ILogger<BlobCacheService> logger)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient("cache");
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(key);

            if (!await blobClient.ExistsAsync())
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            // Get blob properties to check metadata
            var properties = await blobClient.GetPropertiesAsync();

            if (!IsValid(properties.Value.Metadata))
            {
                _logger.LogDebug("Cache expired for key: {Key}", key);
                // Remove expired cache entry
                await blobClient.DeleteIfExistsAsync();
                return null;
            }

            // Cache hit - read the data
            using var stream = await blobClient.OpenReadAsync();
            var data = await JsonSerializer.DeserializeAsync<T>(stream, SharedCommon.JsonOptions);

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading from cache for key: {Key}", key);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(key);
            var expiresAt = DateTime.UtcNow.Add(ttl);

            // Serialize data
            var json = JsonSerializer.Serialize(value, SharedCommon.JsonOptions);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            // Upload with metadata containing expiration time
            var metadata = new Dictionary<string, string>
            {
                [EXPIRY_METADATA_KEY] = expiresAt.ToString("O") // ISO 8601 format
            };

            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                Metadata = metadata,
                Conditions = null, // Overwrite existing
            });

            _logger.LogDebug("Cache set for key: {Key}, expires at: {ExpiresAt}", key, expiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cache for key: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(key);
            await blobClient.DeleteIfExistsAsync();
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache for key: {Key}", key);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(key);

            if (!await blobClient.ExistsAsync())
            {
                return false;
            }

            var properties = await blobClient.GetPropertiesAsync();
            return IsValid(properties.Value.Metadata);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Checks if the cache entry is still valid based on TTL
    /// </summary>
    /// <param name="metadata">Blob metadata containing expiration information</param>
    /// <returns>True if cache entry is valid, false if expired</returns>
    private static bool IsValid(IDictionary<string, string> metadata)
    {
        if (!metadata.TryGetValue(EXPIRY_METADATA_KEY, out var expirationString))
        {
            // No expiration metadata - consider expired
            return false;
        }

        if (!DateTime.TryParse(expirationString, out var expirationTime))
        {
            // Invalid expiration format - consider expired
            return false;
        }

        // Compare in UTC
        return expirationTime.ToUniversalTime() > DateTime.UtcNow;
    }
}