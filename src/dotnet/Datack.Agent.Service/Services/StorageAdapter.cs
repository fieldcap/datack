using System.Globalization;
using Datack.Agent.Services.DataConnections;
using Datack.Common.Models.Internal;

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

    public async Task<IList<BackupFile>> GetFileList(String storageType, String connectionString, String containerName, String rootPath, String? path, CancellationToken cancellationToken)
    {
        var results = new List<BackupFile>();

        var files = await GetConnection(storageType).GetFileList(connectionString, containerName, rootPath, path, cancellationToken);

        foreach (var file in files)
        {
            var fileName = file.Split('/').Last();

            var parts = fileName.Split("-");

            var backupFile = new BackupFile
            {
                FileName = file,
                DatabaseName = parts.Length >= 1 ? parts[0] : null,
                DateTime = parts.Length >= 2 ? DateTimeOffset.ParseExact(parts[1], "yyyyMMddHHmm", CultureInfo.InvariantCulture) : null,
                BackupType = parts.Length >= 3 ? parts[2] : null
            };

            results.Add(backupFile);
        }

        return results;
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