using System.Text.RegularExpressions;

namespace TestProject.Storage;

public class DiskFileBrowser : IFileBrowser, IFileContentData
{
    private readonly IFileValidationService _fileValidationService;
    private readonly IConfiguration _configuration;
    protected const string DefaultPathRoot = "/uploads";
    private string _pathRoot = null;

    public DiskFileBrowser(
        IFileValidationService fileValidationService,
        IConfiguration configuration)
    {
        _fileValidationService = fileValidationService;
        _configuration = configuration;
        _pathRoot = configuration["DiskStorage:PathRoot"] ?? DefaultPathRoot;
        // do some basic security checks, no running in app root folder or parent folders!
        // this basic validation does not support file shares.
        var segments = _pathRoot
            .Replace(@"\", "/") // normalize to forward slashes
            .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries) // break into path segments, remove empties
            .Select(segment => Regex.Replace(segment, "[^a-z0-9]", "")) // only allow alpha numeric folders, prevent security issues
            // don't want any dots to prevent upward path traversal
            // expression could be more expansive to allow other common chars
            .Where(segment => !string.IsNullOrWhiteSpace(segment)) // remove blanks so no double slash results
            .ToList();
        // I would normally put this in a shared library to have one place where this kind of path cleansing can be reused and tested
        // for that matter I'd rather use S3 or Azure blob storage so this is a non-issue for app security and other benefits.
        _pathRoot = string.Join("/", segments); // back to a string
        if (string.IsNullOrWhiteSpace(_pathRoot) || segments.Count < 1)
        {
            throw new ArgumentException("Invalid application configuration for PathRoot, must be relative path.");
        }
        var basePath = Path.Combine(AppContext.BaseDirectory, _pathRoot);
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
    }

    public Task<FileMetadata> GetAsync(string pathAndName)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, _pathRoot, pathAndName ?? "");
        // Normally load file metadata and access rights from a DB here.
        // lets just check that the file exists instead.
        FileInfo fileInfo = new FileInfo(fullPath);
        if (!fileInfo.Exists)
        {
            return null;
        }

        var fileName = Path.GetFileName(pathAndName);
        var ext = Path.GetExtension(pathAndName);
        var file = new FileMetadata
        {
            Directory = fileName.Length == pathAndName.Length ? "" // blank is upload base directory
                : pathAndName.Substring(0, pathAndName.Length - fileName.Length),
            Name = Path.GetFileName(pathAndName),
            Extension = ext,
            Size = fileInfo.Length,
            ContentType = _fileValidationService.DetermineContentType(ext)
        };

        // Why async?  because most implementations (S3, Azure) will require an async call
        return Task.FromResult(file);
    }

    public Task<IList<FileMetadata>> GetAllAsync(string parentPath = null)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, _pathRoot, parentPath ?? "");

        IList<FileMetadata> list = new List<FileMetadata>();
        var dirNames = Directory.GetDirectories(fullPath);
        foreach ( var dirName in dirNames)
        {
            var dirParts = dirName.Split(new[] { '/', '\\' });
            var dirPart = dirParts[^1];
            var subNames = Directory.GetFiles(fullPath);

            list.Add(new FileMetadata
            {
                Name = dirPart,
                Extension = "",
                Directory = parentPath,
                Size = subNames.Length, // number of files
                ContentType = "Directory"
            });
        }

        var fileNames = Directory.GetFiles(fullPath);
        foreach (var fileName in fileNames)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            var nameOnly = Path.GetFileName(fileName);
            var ext = Path.GetExtension(nameOnly);
            list.Add(new FileMetadata
            {
                Name = nameOnly,
                Extension = ext,
                Directory = parentPath,
                Size = fileInfo.Length,
                ContentType = _fileValidationService.DetermineContentType(ext)
            });
        }

        return Task.FromResult(list);
    }

    public async Task<byte[]> GetContentAsync(FileMetadata fileMetadata)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, _pathRoot, fileMetadata.Directory ?? "", fileMetadata.Name);
        FileInfo fileInfo = new FileInfo(fullPath);
        if (!fileInfo.Exists)
        {
            return null;
        }

        return await File.ReadAllBytesAsync(fullPath);
    }

    public async Task UploadAsync(FileMetadata metadata, byte[] content)
    {
        var directoryPath = Path.Combine(AppContext.BaseDirectory, _pathRoot, metadata.Directory ?? "");
        var fullPath = Path.Combine(directoryPath, metadata.Name);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        await File.WriteAllBytesAsync(fullPath, content);
    }
}
