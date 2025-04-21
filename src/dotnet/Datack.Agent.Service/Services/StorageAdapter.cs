using Datack.Agent.Services.DataConnections;

namespace Datack.Agent.Services;

public class StorageAdapter
{
    private readonly AzureBlobStorageConnection _azureBlobStorageConnection;
    private readonly AwsS3Connection _awsS3Connection;

    public StorageAdapter(AzureBlobStorageConnection azureBlobStorageConnection, AwsS3Connection awsS3Connection)
    {
        _azureBlobStorageConnection = azureBlobStorageConnection;
        _awsS3Connection = awsS3Connection;
    }

    public async Task<IList<String>> GetFileList(String storageType, String connectionString, String containerName, String rootPath, String? path, CancellationToken cancellationToken)
    {
        return await GetConnection(storageType).GetFileList(connectionString, containerName, rootPath, path, cancellationToken);
    }

    private IStorageConnection GetConnection(String? storageType)
    {
        if (String.IsNullOrWhiteSpace(storageType))
        {
            throw new("Invalid Database Type");
        }

        if (storageType == "azure")
        {
            return _azureBlobStorageConnection;
        }

        if (storageType == "s3")
        {
            return _awsS3Connection;
        }

        throw new($"Invalid storage type {storageType}");
    }
}