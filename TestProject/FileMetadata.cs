namespace TestProject;

public class FileMetadata
{
    /// <summary>
    /// The file name with extension. Folder path is excluded.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// A reletive directory to the storage root folder where the file <see cref="Name"/> can be found.
    /// </summary>
    public string Directory { get; set; }
    /// <summary>
    /// The extension of the file <see cref="Name"/> only, including the leading period.
    /// </summary>
    public string Extension { get; set; }
    /// <summary>
    /// The number of bytes in the content.
    /// </summary>
    public long Size { get; set; }
    /// <summary>
    /// The Content-Type header value of the file contents.
    /// </summary>
    public string ContentType { get; set; }
}
