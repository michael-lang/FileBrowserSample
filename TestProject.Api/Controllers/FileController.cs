using Microsoft.AspNetCore.Mvc;

namespace TestProject.Controllers;

// TODO: In a real application, check file based access rules for current user.
//[Authorize(Policy = "ApiUser")] // plus code in each action to verify access

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileValidationService _fileValidationService;
    private readonly IFileBrowser _browser;
    private readonly IFileContentData _contentData;
    private readonly ILogger<FileController> _logger;

    public FileController(
        IFileValidationService fileValidationService,
        IFileBrowser browser,
        IFileContentData contentData,
        ILogger<FileController> logger
        )
    {
        _fileValidationService = fileValidationService;
        _browser = browser;
        _contentData = contentData;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> QueryAsync([FromQuery]string path = null)
    {
        
        var list = await _browser.GetAllAsync(path);
        return Ok(list); // 0 or more items
    }

    [HttpGet("content")]
    public async Task<IActionResult> DownloadAsync([FromQuery] string path = null)
    {
        var file = await _browser.GetAsync(path);
        if (file == null) 
        {
            return NotFound(); // never existed. (it is not in the "database".)
        }

        var content = await _contentData.GetContentAsync(file);
        if (content == null || content.Length == 0)
        {
            return UnprocessableEntity("File has been removed.");
        }

        return File(content, file.ContentType, file.Name);
    }

    [HttpPost("")]
    public async Task<IActionResult> OnPostUpload(List<IFormFile> model, [FromQuery]string path = null)
    {
        if (!ModelState.IsValid || model == null)
            return BadRequest();

        var files = new List<FileMetadata>();
        foreach (var formFile in model)
        {   // accepting an array, but usually this will be one file per POST request.
            if (formFile.Length <= 0)
                continue;

            FileMetadata fil;
            using (var memoryStream = new System.IO.MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);
                fil = new FileMetadata
                {
                    ContentType = formFile.ContentType,
                    Extension = System.IO.Path.GetExtension(formFile.FileName),
                    Size = memoryStream.Length,
                    Name = formFile.FileName,
                    Directory = path
                };
                var content = memoryStream.ToArray();
                var isValid = _fileValidationService.IsValid(fil.Name, content);
                if (!isValid)
                {   // either an unsupported format, or a corrupt file, or potentially malicious
                    return UnprocessableEntity($"Unknown format in file '{fil.Name}'.");
                    // exit with 'return' even if there are more files to process. Alternatively, 
                    // This could skip current and add to a list of validation errors instead.
                    // a mix of accepted and failed files would just be a more complex return type
                }

                await _contentData.UploadAsync(fil, content);

                files.Add(fil);
            }
        }
        return Ok(files); // return the metadata of files just uploaded for optional ui display update
    }
}