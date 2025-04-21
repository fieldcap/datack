namespace Datack.Agent.Services.DataConnections;

public interface IStorageConnection
{
    Task<IList<String>> GetFileList(String connectionString, String containerName, String rootPath, String? path, CancellationToken cancellationToken);
}