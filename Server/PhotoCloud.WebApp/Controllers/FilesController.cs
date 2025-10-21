using System.Net.Mime;
using Infractructure.DTO.WebAppClientDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoCloud.Controllers;
using PhotoCloud.Interfaces;

[ApiController]
[Route("api/files")]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = "v1")]
public class FilesController : BaseController
{
    private readonly IFileFolderService _fileFolderService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        ICurrentUserService currentUserService,
        IFileFolderService fileFolderService,
        ILogger<FilesController> logger) : base(currentUserService)
    {
        _fileFolderService = fileFolderService;
        _logger = logger;
    }

    /// <summary>
    /// Завершение загрузки файла в Blob storage.
    /// Сервер проверит наличие blob, сверит метаданные и поставит задачу на обработку (если задано).
    /// </summary>
    /// <param name="completeUploadDto">DTO с идентификатором файла и опциональным идемпотентным ключом.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Информация о состоянии файла и (опционально) созданной задаче.</returns>
    /// <response code="200">Файл подтверждён и текущее состояние возвращено (например Uploaded).</response>
    /// <response code="202">Файл принят в обработку — создана асинхронная задача; возвращается JobId.</response>
    /// <response code="400">Некорректный запрос (валидация) или размер/тип файла не соответствует сохранённым ожиданиям.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="403">Пользователь не имеет прав на этот файл.</response>
    /// <response code="404">Файл с указанным идентификатором не найден.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpPost("complete-upload")]
    [ProducesResponseType(typeof(CompleteUploadResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteUpload([FromBody] CompleteUploadDTO completeUploadDto, CancellationToken cancellationToken)
    {
        if (completeUploadDto == null)
            return BadRequest(new ProblemDetails { Title = "Request body is required." });

        if (!ModelState.IsValid)
            return BadRequest(new ValidationProblemDetails(ModelState));

        if (CurrentUserId == null)
            return Unauthorized(new ProblemDetails { Title = "Unauthorized" });

        // Сервис должен реализовать все проверки: владельца, верификацию blob, идемпотентность, создание Job/Outbox и т.д.
        var result = await _fileFolderService.CompleteUploadAsync(
            completeUploadDto.FileId,
            completeUploadDto.IdempotencyKey,
            CurrentUserId.Value,
            cancellationToken);

        if (result == null)
            return NotFound(new ProblemDetails { Title = "File not found" });

        if (string.Equals(result.FileStatus, "Processing", StringComparison.OrdinalIgnoreCase)
            && result.JobId.HasValue)
        {
            var location = Url.Action("GetJob", "Jobs", new { id = result.JobId }, Request.Scheme);
            if (!string.IsNullOrEmpty(location))
                Response.Headers["Location"] = location;

            return Accepted(new { result.FileId, result.JobId, result.FileStatus, result.Message });
        }

        return Ok(result);
    }
}
