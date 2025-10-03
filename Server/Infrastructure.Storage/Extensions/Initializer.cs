using Infrastructure.Storage.Configuration;
using Infrastructure.Storage.Interfaces;
using Infrastructure.Storage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Storage.Extensions;

public static class Initializer
{
    public static IServiceCollection AddAzureStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureStorageConfiguration>(configuration.GetSection("AzureStorage"));
        services.AddSingleton<IAzureService, AzureService>();
        services.AddHostedService<BlobInitializer>();
        return services;
    }
}