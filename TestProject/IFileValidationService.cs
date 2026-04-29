using static System.Net.Mime.MediaTypeNames;

namespace TestProject;

/// <summary>
/// Validates that a file upload is what it claims to be. This does not scan for malware or viruses.
/// </summary>
public interface IFileValidationService
{
    /// <summary>
    /// Determines if the file is the type it claims to be in the file extension.
    /// </summary>
    /// <param name="uploadedFile"></param>
    /// <returns></returns>
    bool IsValid(string fileName, byte[] contents);
    /// <summary>
    /// Determine the Content-Type of a file based on the extension.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    string DetermineContentType(string extension);
}

public class DefaultFileValidationService : IFileValidationService
{
    /// <summary>
    /// A definition of possible header bytes for all supported upload file types
    /// to be used in validating that a file is the type it is claimed to be.
    /// </summary>
    private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new Dictionary<string, List<byte[]>>
    {
        // image formats:
        { ".bmp", new List<byte[]>{ new byte[]{ 0x42, 0x4D } } },
        { ".png", new List<byte[]>{ new byte[]{ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".jpg",  new List<byte[]> // TODO: there are more possible header bytes combos.
            {
                new byte[]{ 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[]{ 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[]{ 0xFF, 0xD8, 0xFF, 0xE3 },
                new byte[]{ 0xFF, 0xD8, 0xFF, 0xDB },
                new byte[]{ 0xFF, 0xD8, 0xFF, 0xEE },
                new byte[]{ 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A }, // jpg 2000
            }
        },
        { ".jpeg",  new List<byte[]>
            {
                new byte[]{ 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[]{ 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[]{ 0xFF, 0xD8, 0xFF, 0xE3 },
                new byte[]{ 0xFF, 0xD8, 0xFF, 0xDB },
                new byte[]{ 0xFF, 0xD8, 0xFF, 0xEE },
                new byte[]{ 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A }, // jpg 2000
            }
        },
        { ".jpg2",  new List<byte[]>
            {
                new byte[]{ 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A }, // jpg 2000
            }
        },
        { ".gif",  new List<byte[]>
            {
                new byte[]{ 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 },
                new byte[]{ 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 },
            }
        },
        { ".tif",  new List<byte[]>
            {
                new byte[]{ 0x49, 0x49, 0x2A, 0x00 },
                new byte[]{ 0x4D, 0x4D, 0x00, 0x2A },
                new byte[]{ 0x49, 0x49, 0x2B, 0x00 },
                new byte[]{ 0x4D, 0x4D, 0x00, 0x2B },
            }
        },
        { ".tiff",  new List<byte[]>
            {
                new byte[]{ 0x49, 0x49, 0x2A, 0x00 },
                new byte[]{ 0x4D, 0x4D, 0x00, 0x2A },
                new byte[]{ 0x49, 0x49, 0x2B, 0x00 },
                new byte[]{ 0x4D, 0x4D, 0x00, 0x2B },
            }
        },
        { ".webp", new List<byte[]>{ new byte[]{ 0x52, 0x49, 0x46, 0x46 } } },
        { ".ico", new List<byte[]>{ new byte[]{ 0x00, 0x00, 0x01, 0x00 } } },
        // video / audio
        { ".3gp", new List<byte[]>{ new byte[]{ 0x66, 0x74, 0x79, 0x70, 0x33, 0x67 } } },
        { ".3g2", new List<byte[]>{ new byte[]{ 0x66, 0x74, 0x79, 0x70, 0x33, 0x67 } } },
        { ".asf", new List<byte[]>{ new byte[]{ 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C } } },
        { ".wma", new List<byte[]>{ new byte[]{ 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C } } },
        { ".wmv", new List<byte[]>{ new byte[]{ 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C } } },
        { ".avi", new List<byte[]>{ new byte[]{ 0x52, 0x49, 0x46, 0x46 } } },
        { ".wav", new List<byte[]>{ new byte[]{ 0x52, 0x49, 0x46, 0x46 } } },
        { ".mp3", new List<byte[]>{
            new byte[]{ 0xFF, 0xFB },
            new byte[]{ 0xFF, 0xF3 },
            new byte[]{ 0xFF, 0xF2 },
            new byte[]{ 0x49, 0x44, 0x33 },
        } },
        { ".mp4", new List<byte[]>{
            new byte[]{ 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D },
            new byte[]{ 0x66, 0x74, 0x79, 0x70, 0x4D, 0x53, 0x4E, 0x56 },
        } },
        { ".mpg", new List<byte[]>{
            new byte[]{ 0x47 },
            new byte[]{ 0x00, 0x00, 0x01, 0xBA },
            new byte[]{ 0x00, 0x00, 0x01, 0xB3 }
        } },
        { ".mpeg", new List<byte[]>{
            new byte[]{ 0x47 },
            new byte[]{ 0x00, 0x00, 0x01, 0xBA },
            new byte[]{ 0x00, 0x00, 0x01, 0xB3 }
        } },
        // text
        { ".txt", new List<byte[]>{
            new byte[]{ 0xEF, 0xEB, 0xBF }, // UTF-8
            new byte[]{ 0xEF, 0xBB, 0xBF }, // UTF-8
            new byte[]{ 0xFF, 0xFE }, //UTF-16LE
            new byte[]{ 0xFE, 0xFF }, //UTF-16BE
            new byte[]{ 0x00, 0x00, 0xFE, 0xFF }, // UTF-32BE
            new byte[]{ 0x2B, 0x2F, 0x76, 0x38 }, // UTF-7
            new byte[]{ 0x2B, 0x2F, 0x76, 0x39 }, // UTF-7
            new byte[]{ 0x2B, 0x2F, 0x76, 0x2B }, // UTF-7
            new byte[]{ 0x2B, 0x2F, 0x76, 0x2F }, // UTF-7
            new byte[]{ 0x0E, 0xFE, 0xFF }, // SCSU byte order mark compressed
        } },
        // non-media common uploads
        { ".pdf", new List<byte[]>{ new byte[]{ 0x25, 0x50, 0x44, 0x46, 0x2D } } }, // document
        { ".eps", new List<byte[]>{ new byte[]{ 0x25, 0x21, 0x50, 0x53, 0x2D, 0x41, 0x64, 0x6F, 0x62, 0x65, 0x2D, 0x33, 0x2E, 0x30, 0x20, 0x45, 0x50, 0x53, 0x46, 0x2D, 0x33, 0x2E, 0x30 } } }, // v3.0
        { ".epsf", new List<byte[]>{ new byte[]{ 0x25, 0x21, 0x50, 0x53, 0x2D, 0x41, 0x64, 0x6F, 0x62, 0x65, 0x2D, 0x33, 0x2E, 0x31, 0x20, 0x45, 0x50, 0x53, 0x46, 0x2D, 0x33, 0x2E, 0x30 } } }, // v3.1
        { ".psd", new List<byte[]>{ new byte[]{ 0x38, 0x42, 0x50, 0x53 } } }, // photoshop
        { ".doc", new List<byte[]>{ new byte[]{ 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
        { ".xls", new List<byte[]>{ new byte[]{ 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
        { ".ppt", new List<byte[]>{ new byte[]{ 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
        { ".rtf", new List<byte[]>{ new byte[]{ 0x7B, 0x5C, 0x72, 0x74, 0x66, 0x31 } } },
        
        // NOTE: delete or comment out any file formats you don't want to support. Add others you want to support.
        // more file extensions - https://en.wikipedia.org/wiki/List_of_file_signatures
    };
    private static readonly Dictionary<string, string> _contentTypes = new Dictionary<string, string>{
        {".bmp", "image/bmp" },
        {".png", "image/png" },
        {".jpg", "image/jpeg" },
        {".jpeg", "image/jpeg" },
        {".jpg2", "image/jpeg" },
        {".gif", "image/gif" },
        {".tif", "image/tif" },
        {".tiff", "image/tiff" },
        {".webp", "image/webp" },
        {".ico", "image/vnd.microsoft.icon" },
        {".3gp", "audio/3gpp" },
        {".3g2", "video/3gpp2" },
        {".asf", "video/x-ms-asf" },
        {".wma", "audio/x-ms-wma" },
        {".wmv", "video/x-ms-asf" },
        {".avi", "video/avi" },
        {".wav", "audio/wav" },
        {".mp3", "audio/mpeg" },
        {".mp4", "video/mp4" },
        {".mpg", "video/mpeg" },
        {".mpeg", "video/mpeg" },
        {".txt", "text/plain" },
        {".pdf", "application/pdf" },
        {".eps", "application/eps" },
        {".epsf", "application/epsf" },
        {".psd", "application/psd" },
        {".doc", "application/msword" },
        {".xls", "application/vnd.ms-excel" },
        {".ppt", "application/vnd.ms-powerpoint" },
        {".rtf", "application/rtf" },
    };

    public bool IsValid(string fileName, byte[] contents)
    {
        string ext = Path.GetExtension(fileName);

        if (!_fileSignatures.ContainsKey(ext))
        {
            return false;
        }

        var signatures = _fileSignatures[ext];
        var maxLength = Math.Min(contents.Length, signatures.Max(m => m.Length));
        var headerBytes = contents.Take(maxLength).ToArray();

        return signatures.Any(signature =>
            headerBytes.Take(signature.Length).SequenceEqual(signature));
    }
    public string DetermineContentType(string extension)
    {
        if (!_contentTypes.ContainsKey(extension))
        {
            return "application/" + extension.TrimStart('.'); // fallback value
        }
        return _contentTypes[extension];
    }
}