namespace PhotoCloud.Extensions;

public static class Middleware
{
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiExceptionHandlingMiddleware>();
    }
}