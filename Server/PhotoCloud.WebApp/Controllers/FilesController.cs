using Infractructure.DTO.WebAppClientDTO;
using Microsoft.AspNetCore.Mvc;
using PhotoCloud.Interfaces;

namespace PhotoCloud.Controllers;

[ApiController]
[Route("[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileFolderService _fileFolderService;

    public FilesController(IFileFolderService fileFolderService)
    {
        _fileFolderService = fileFolderService;
    }

    public async Task<IActionResult> CompleteUpload([FromBody] CompleteUploadDTO completeUploadDto)
    {
    }
}