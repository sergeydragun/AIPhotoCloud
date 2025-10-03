using Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;

namespace PhotoCloud
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override( "Microsoft.AspNetCore", LogEventLevel.Warning )
                .MinimumLevel.Override( "Microsoft.Extensions.Hosting", LogEventLevel.Information )
                .MinimumLevel.Override( "Microsoft.Hosting", LogEventLevel.Information )
                .CreateLogger();
            

            builder.Services.AddSerilog();
            
            builder.Services.AddControllers();
            
            builder.Services.AddDatabaseContext(builder.Configuration);
            
            builder.Services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            var app = builder.Build();
            app.UseSerilogRequestLogging();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.Run();
        }
    }
}