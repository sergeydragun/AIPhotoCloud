using Infrastructure.Data.DbContext;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Interfaces;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data.Extensions;

public static class Extensions
{
    public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        string connection = configuration.GetConnectionString("DefaultConnection")!;

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connection));

        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDetectedObjectsRepository, DetectedObjectsRepository>();
        services.AddScoped<IFileModelRepository, FileModelRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        return services;
    }
}