namespace TestProject;

public interface IFileBrowser
{
    /// <summary>
    /// Gets a single file matching the supplied path and file name, relative to the storage root.
    /// </summary>
    /// <param name="pathAndName"></param>
    /// <returns></returns>
    Task<FileMetadata> GetAsync(string pathAndName);
    /// <summary>
    /// Gets all files directly within a specific parent folder path.
    /// </summary>
    /// <param name="parentPath">Null for root folder, or a valid path.</param>
    /// <returns></returns>
    Task<IList<FileMetadata>> GetAllAsync(string parentPath = null);
}
