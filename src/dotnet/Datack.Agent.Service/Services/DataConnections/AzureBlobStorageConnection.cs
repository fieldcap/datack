using Azure.Storage.Blobs;

namespace Datack.Agent.Services.DataConnections;

public class AzureBlobStorageConnection : IStorageConnection
{
    private readonly DataProtector _dataProtector;

    public AzureBlobStorageConnection(DataProtector dataProtector)
    {
        _dataProtector = dataProtector;
    }

    public async Task<IList<String>> GetFileList(String connectionString, String containerName, String rootPath, String? path, CancellationToken cancellationToken)
    {
        connectionString = _dataProtector.Decrypt(connectionString);

        var blobServiceClient = new BlobServiceClient(connectionString);

        // List all files in the container
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        var blobItems = new List<String>();

        var prefix = rootPath.TrimEnd('/') + "/";

        if (!String.IsNullOrWhiteSpace(path))
        {
            prefix = path.TrimEnd('/') + "/";
        }

        await foreach (var blobItem in blobContainerClient.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/", cancellationToken: cancellationToken))
        {
            var name = blobItem.Blob != null ? blobItem.Blob.Name : blobItem.Prefix;
            blobItems.Add(name);
        }

        return blobItems;
    }
}