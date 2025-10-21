using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

public class ApiExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ApiExceptionHandlingMiddleware(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, _logger);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
    {
        // Default
        var status = StatusCodes.Status500InternalServerError;
        ProblemDetails problem;

        switch (exception)
        {
            case UnauthorizedAccessException _:
                status = StatusCodes.Status403Forbidden;
                problem = new ProblemDetails { Title = "Forbidden", Detail = exception.Message, Status = status };
                break;

            case FileNotFoundException _:
                status = StatusCodes.Status404NotFound;
                problem = new ProblemDetails { Title = "Not Found", Detail = exception.Message, Status = status };
                break;

            case InvalidOperationException _:
                status = StatusCodes.Status400BadRequest;
                problem = new ProblemDetails { Title = "Bad Request", Detail = exception.Message, Status = status };
                break;

            case ArgumentException _:
                status = StatusCodes.Status400BadRequest;
                problem = new ProblemDetails { Title = "Invalid argument", Detail = exception.Message, Status = status };
                break;

            case DbUpdateConcurrencyException _:
                status = StatusCodes.Status409Conflict;
                problem = new ProblemDetails { Title = "Conflict", Detail = exception.Message, Status = status };
                break;

            // Тут можно расширять — например свои DomainException/ValidationException с кодами
            default:
                status = StatusCodes.Status500InternalServerError;
                problem = new ProblemDetails { Title = "An unexpected error occurred.", Detail = exception.Message, Status = status };
                break;
        }

        // Логируем подробности (stack trace) для внутренней диагностики
        logger.Error(exception, "Unhandled exception");

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(problem, options);
        return context.Response.WriteAsync(json);
    }
}
