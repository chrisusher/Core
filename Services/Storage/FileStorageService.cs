using System.Globalization;
using System.Text;
using ChrisUsher.Core.Services.Interfaces;
using ChrisUsher.Core.Shared;
using Microsoft.Extensions.Configuration;

namespace ChrisUsher.Core.Services.Storage;

public class FileStorageService : IStorageService
{
    private string? _rootFolderPath;
    private string? _folderPath;

    public FileStorageService(IConfiguration configuration)
    {
        _rootFolderPath = configuration["Storage:RootFolder"];

        // Throw if _rootFolder is null
        ArgumentNullException.ThrowIfNull(_rootFolderPath);

        if (!Directory.Exists(_rootFolderPath!))
        {
            Directory.CreateDirectory(_rootFolderPath!);
        }
    }

    public string FolderName
    {
        get
        {
            if (_folderPath is null)
            {
                throw new InvalidOperationException("FolderName has not been set.");
            }
            return _folderPath;
        }
        set
        {
            _folderPath = Path.Combine(_rootFolderPath!, value!);

            if (!Directory.Exists(_folderPath!))
            {
                Directory.CreateDirectory(_folderPath!);
            }
        }
    }

    public void Dispose()
    {
        _rootFolderPath = null;
        _folderPath = null;
    }

    public async Task<bool> FileExistsAsync(string path)
    {
        return await Task.FromResult(File.Exists(path));
    }

    public async Task<(bool, string?)> FileExistsAsync(string prefix, DateTime date)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(prefix));
        }

        if (prefix.EndsWith('/'))
        {
            prefix = prefix.TrimEnd('/');
        }

        var latestFile = await GetLatestFileAsync(prefix, date);

        if (latestFile is null)
        {
            return (false, null);
        }

        return (true, latestFile);
    }

    public async Task<List<string>> GetFilesAsync(string path)
    {
        var pathWithParentFolder = Path.Join(FolderName, path);

        if (!Directory.Exists(pathWithParentFolder))
        {
            Directory.CreateDirectory(pathWithParentFolder);
        }

        var files = Directory.GetFiles(pathWithParentFolder, "*", SearchOption.TopDirectoryOnly).ToList();

        return await Task.FromResult(files);
    }

    public async Task<T?> ReadFileAsync<T>(string path)
    {
        var checkedPath = path;

        if (!checkedPath.StartsWith(FolderName))
        {
            checkedPath = Path.Join(FolderName, path);
        }

        if (!await FileExistsAsync(checkedPath))
        {
            return default;
        }

        using var stream = File.OpenRead(checkedPath);
        return await JsonSerializer.DeserializeAsync<T>(stream, SharedCommon.JsonOptions);
    }

    public async Task<T?> ReadLatestFileAsync<T>(string folderPath, DateTime minDate)
    {
        var latestFiles = await GetLatestFilesAsync(folderPath, minDate);

        if (latestFiles is null)
        {
            return default;
        }

        if (latestFiles.Count == 0)
        {
            return default;
        }

        return await ReadFileAsync<T>(latestFiles.First());
    }

    public async Task<T?> ReadLatestFileAsync<T>(string folderPath)
    {
        var latestFile = await GetLatestFileAsync(folderPath);

        if (latestFile is null)
        {
            return default;
        }

        return await ReadFileAsync<T>(latestFile);
    }

    public async Task SaveFileAsync<T>(T data, string path)
    {
        var pathWithParentFolder = Path.Join(FolderName, path);

        var fileInfo = new FileInfo(pathWithParentFolder);

        if (!Directory.Exists(fileInfo.DirectoryName!))
        {
            Directory.CreateDirectory(fileInfo.DirectoryName!);
        }

        // Serialise data to JSON and write to file.
        var json = JsonSerializer.Serialize(data, SharedCommon.JsonOptions);

        await File.WriteAllTextAsync(pathWithParentFolder, json, Encoding.UTF8);
    }

    private async Task<List<string>> GetLatestFilesAsync(string folderPath, DateTime minDate)
    {
        var files = await GetFilesAsync(Path.Join(folderPath, minDate.ToString("yyyy-MM-dd")));

        var latestFiles = files
            .Where(x =>
            {
                var datePart = x.Split("/").Last();
                var fileName = Path.GetFileNameWithoutExtension(datePart);

                return DateTime.ParseExact(fileName, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture) >= minDate;
            })
            .OrderByDescending(x =>
            {
                // Use the File Last Updated Date as the sort key.
                var fileInfo = new FileInfo(x);

                return fileInfo.LastWriteTimeUtc;
            })
            .ToList();

        return latestFiles;
    }

    private async Task<string?> GetLatestFileAsync(string folderPath, DateTime minDate)
    {
        var files = await GetFilesAsync(Path.Join(folderPath, minDate.ToString("yyyy-MM-dd")));

        var latestQuote = files
            .Where(x =>
            {
                var datePart = x.Split("/").Last();
                var fileName = Path.GetFileNameWithoutExtension(datePart);

                return DateTime.ParseExact(fileName, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture) >= minDate;
            })
            .OrderByDescending(x =>
            {
                var datePart = x.Split("/").Last();
                var fileName = Path.GetFileNameWithoutExtension(datePart);

                return DateTime.ParseExact(fileName, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
            })
            .FirstOrDefault();

        return latestQuote;
    }

    private async Task<string?> GetLatestFileAsync(string folderPath)
    {
        var files = await GetFilesAsync(folderPath);

        var latestQuote = files
            .OrderByDescending(x =>
            {
                // Use the File Last Updated Date as the sort key.
                var fileInfo = new FileInfo(x);
                return fileInfo.LastWriteTimeUtc;
            })
            .FirstOrDefault();

        return latestQuote;
    }
}