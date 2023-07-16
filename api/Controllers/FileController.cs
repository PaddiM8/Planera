using Microsoft.AspNetCore.Mvc;
using Planera.Data.Files;

namespace Planera.Controllers;

[ApiController]
[Route("files")]
public class FileController : ControllerBase
{
    private readonly IFileStorage _fileStorage;

    public FileController(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
    }

    [HttpGet("{directory}/{name}")]
    public async Task<IActionResult> Get(string directory, string name, string mimeType, string? param = null)
    {
        var bytes = await _fileStorage.ReadAsync(directory, name, param);
        return bytes == null
            ? NotFound()
            : File(bytes, mimeType);
    }
}