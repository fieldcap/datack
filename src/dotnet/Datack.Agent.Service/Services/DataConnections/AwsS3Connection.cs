namespace Datack.Agent.Services.DataConnections;

public class AwsS3Connection : IStorageConnection
{
    public async Task<IList<String>> GetFileList(String connectionString, String containerName, String rootPath, String? path, CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);

        return [];
    }
}