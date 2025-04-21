using Datack.Agent.Models.Internal;
using Datack.Common.Models.Internal;

namespace Datack.Agent.Services.DataConnections;

public interface IDatabaseConnection
{
    Task Test(String connectionString, CancellationToken cancellationToken);
    Task<IList<Database>> GetDatabaseList(String connectionString, CancellationToken cancellationToken);

    Task CreateBackup(String connectionString,
                      String databaseName,
                      String? backupType,
                      String? password,
                      String? options,
                      String destinationFilePath,
                      Action<DatabaseProgressEvent> progressCallback,
                      CancellationToken cancellationToken);

    Task RestoreBackup(String connectionString,
                       String databaseName,
                       String? databaseLocation,
                       String? password,
                       String? options,
                       String sourceFilePath,
                       Action<DatabaseProgressEvent> progressCallback,
                       CancellationToken cancellationToken);
}