using Microsoft.AspNetCore.Mvc;
using Planera.Api.Data.Files;

namespace Planera.Api.Controllers;

[ApiController]
[Route("files")]
public class FileController(IFileStorage fileStorage) : ControllerBase
{
    private readonly IFileStorage _fileStorage = fileStorage;

    [HttpGet("{directory}/{name}")]
    public async Task<IActionResult> Get(string directory, string name, string mimeType, string? param = null)
    {
        var bytes = await _fileStorage.ReadAsync(directory, name, param);
        return bytes == null
            ? NotFound()
            : File(bytes, mimeType);
    }
}