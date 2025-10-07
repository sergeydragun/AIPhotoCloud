using Infrastructure.Storage.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Storage.Services;

public class BlobInitializer : IHostedService
{
    private readonly IAzureService _azureService;

    public BlobInitializer(IAzureService azureService)
    {
        _azureService = azureService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _azureService.EnsureContainerExistsAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}