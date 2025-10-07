using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PhotoCloud.Interfaces;

namespace PhotoCloud.Controllers;

[ApiController]
[Route("api/events/blob")]
public class BlobEventsController : ControllerBase
{
    private readonly IFileFolderService _fileFolderService;

    public BlobEventsController(IFileFolderService fileFolderService)
    {
        _fileFolderService = fileFolderService;
    }

    [HttpPost]
    public async Task<IActionResult> Events([FromBody] JsonElement json)
    {
        var jsonResult = await _fileFolderService.ProcessBlobEvent(json);

        return Ok(jsonResult);
    }
}