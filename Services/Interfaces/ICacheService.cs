namespace ChrisUsher.Core.Services.Interfaces;

/// <summary>
/// Interface for TTL-based caching service using Azure Blob Storage
/// Provides methods to cache data with configurable time-to-live (TTL) expiration
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets cached data if it exists and hasn't expired
    /// </summary>
    /// <typeparam name="T">Type of data to retrieve</typeparam>
    /// <param name="key">Cache key (blob path)</param>
    /// <returns>Cached data if found and not expired, otherwise null</returns>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Sets data in cache with specified TTL
    /// </summary>
    /// <typeparam name="T">Type of data to cache</typeparam>
    /// <param name="key">Cache key (blob path)</param>
    /// <param name="value">Data to cache</param>
    /// <param name="ttl">Time-to-live duration</param>
    Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class;

    /// <summary>
    /// Removes data from cache
    /// </summary>
    /// <param name="key">Cache key (blob path)</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Checks if cache entry exists and is not expired
    /// </summary>
    /// <param name="key">Cache key (blob path)</param>
    /// <returns>True if cache entry exists and is valid, false otherwise</returns>
    Task<bool> ExistsAsync(string key);
}