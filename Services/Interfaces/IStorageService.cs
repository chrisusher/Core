namespace ChrisUsher.Core.Services.Interfaces;

public interface IStorageService : IDisposable
{
    public string FolderName { get; set; }

    public Task<bool> FileExistsAsync(string path);

    /// <summary>
    /// Checks if a file exists based on a prefix and a specific date.
    /// </summary>
    /// <param name="prefix">The prefix to search for in the file name.</param>
    /// <param name="date">The date associated with the file.</param>
    /// <returns>A tuple containing a boolean indicating existence and an optional string with the file path.</returns>
    public Task<(bool, string?)> FileExistsAsync(string prefix, DateTime date);

    /// <summary>
    /// Retrieves a list of file names from the specified path.
    /// </summary>
    /// <param name="path">The path from which to retrieve file names.</param>
    /// <returns>A list of file names.</returns>
    public Task<List<string>> GetFilesAsync(string path);

    /// <summary>
    /// Reads a file asynchronously and deserializes its content into an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
    /// <param name="path">The path of the file to read.</param>
    /// <returns>A task representing the asynchronous operation, with the deserialized object or null if the file is not found.</returns>
    public Task<T?> ReadFileAsync<T>(string path);

    /// <summary>
    /// Reads the latest file from a specified folder based on a given date and deserializes its content into an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
    /// <param name="folderPath">The path of the folder to search for files.</param>
    /// <param name="quoteDate">The date associated with the file.</param>
    /// <returns>A task representing the asynchronous operation, with the deserialized object or null if no matching file is found.</returns>
    public Task<T?> ReadLatestFileAsync<T>(string folderPath, DateTime quoteDate);

    /// <summary>
    /// Reads the latest file from a specified folder and deserializes its content into an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
    /// <param name="folderPath">The path of the folder to search for files.</param>
    /// <returns>A task representing the asynchronous operation, with the deserialized object or null if no matching file is found.</returns>
    public Task<T?> ReadLatestFileAsync<T>(string folderPath);

    /// <summary>
    /// Saves an object of type T to a specified file path asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the object to save.</typeparam>
    /// <param name="data">The object to save.</param>
    /// <param name="path">The path of the file to save.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SaveFileAsync<T>(T data, string path);
}
