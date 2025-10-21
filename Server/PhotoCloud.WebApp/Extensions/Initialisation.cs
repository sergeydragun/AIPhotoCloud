using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PhotoCloud.Configuration;
using PhotoCloud.Interfaces;
using PhotoCloud.Services;
using PhotoCloud.Validators;
using PhotoCloud.Validators.Filters;

namespace PhotoCloud.Extensions;

public static class Initialisation
{
    public static IServiceCollection AddPhotoCloudAuthentification(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Jwt"));

        return services;
    }

    public static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
        
        services.AddScoped<FluentValidationActionFilter>();

        return services;
    }

    public static IServiceCollection AddOpenApiExt(this IServiceCollection services)
    {
        services.AddOpenApi();
        
        return services;
    }
}