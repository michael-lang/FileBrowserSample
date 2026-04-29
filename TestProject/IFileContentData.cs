namespace TestProject;

public interface IFileContentData
{
    /// <summary>
    /// Loads the file content of a previously uploaded file.
    /// </summary>
    /// <param name="fileMetadata">Details about the source folder and file name.</param>
    /// <returns></returns>
    Task<byte[]> GetContentAsync(FileMetadata fileMetadata);
    /// <summary>
    /// Uploads a file. (no progress reporting)
    /// </summary>
    /// <param name="metadata">Details about the destination folder and file name.</param>
    /// <param name="content">The content bytes.</param>
    /// <returns></returns>
    Task UploadAsync(FileMetadata metadata, byte[] content);
}
