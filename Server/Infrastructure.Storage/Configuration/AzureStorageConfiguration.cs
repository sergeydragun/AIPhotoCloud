namespace Infrastructure.Storage.Configuration;

public class AzureStorageConfiguration
{
    public string ConnectionString { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public string AccountKey { get; set; } = null!;
    public string ContainerName { get; set; } = "mycontainer";
    public string AzureHost { get; set; } = null!;
}