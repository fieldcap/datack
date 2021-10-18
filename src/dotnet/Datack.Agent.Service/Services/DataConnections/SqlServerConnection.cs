using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Datack.Agent.Models.Internal;
using Datack.Common.Models.Internal;
using Microsoft.Data.SqlClient;

namespace Datack.Agent.Services.DataConnections
{
    public class SqlServerConnection
    {
        private static String BuildConnectionString(ServerDbSettings serverDbSettings)
        {
            return $"Server={serverDbSettings.Server};User Id={serverDbSettings.UserName};Password={serverDbSettings.Password};Timeout={serverDbSettings.ConnectionTimeout}";
        }
        
        public async Task Test(ServerDbSettings serverDbSettings, CancellationToken cancellationToken)
        {
            var connectionString = BuildConnectionString(serverDbSettings);
            
            await using var sqlConnection = new SqlConnection(connectionString);

            await sqlConnection.OpenAsync(cancellationToken);
        }

        public async Task<IList<Database>> GetDatabaseList(ServerDbSettings serverDbSettings, CancellationToken cancellationToken)
        {
            await using var sqlConnection = new SqlConnection(BuildConnectionString(serverDbSettings));

            await sqlConnection.OpenAsync(cancellationToken);

            var result = await sqlConnection.QueryAsync<Database>(@"SELECT 
	name AS 'DatabaseName', 
	HAS_PERMS_BY_NAME(name, 'DATABASE', 'BACKUP DATABASE') AS 'HasAccess' 
FROM 
	sys.databases");

            return result.ToList();
        }

        public async Task<IList<File>> GetFileList(ServerDbSettings serverDbSettings, CancellationToken cancellationToken)
        {
            await using var sqlConnection = new SqlConnection(BuildConnectionString(serverDbSettings));

            await sqlConnection.OpenAsync(cancellationToken);

            var result = await sqlConnection.QueryAsync<File>(@"SELECT 
	DB_NAME(database_id) AS 'DatabaseName',
	[type] as 'Type',
	[physical_name] as 'PhysicalName',
	size as 'Size'
FROM
	sys.master_files
ORDER BY 
	DB_NAME(database_id)");

            return result.ToList();
        }

        public async Task CreateBackup(ServerDbSettings serverDbSettings, String databaseName, String destinationFilePath, Action<DatabaseProgressEvent> progressCallback, CancellationToken cancellationToken)
        {
            await using var sqlConnection = new SqlConnection(BuildConnectionString(serverDbSettings));

            var backupName = $"{databaseName} Backup";

            sqlConnection.InfoMessage += (_, args) =>
            {
                progressCallback?.Invoke(new DatabaseProgressEvent
                {
                    Message = args.Message,
                    Source = args.Source
                });
            };

            var command = new CommandDefinition($@"BACKUP DATABASE @DatabaseName 
TO DISK = @FilePath WITH NOFORMAT, 
INIT,
NAME = @BackupName, 
SKIP, 
NOREWIND, 
NOUNLOAD,
STATS = 10",
                                                new
                                                {
                                                    DatabaseName = databaseName,
                                                    FilePath = destinationFilePath,
                                                    BackupName = backupName
                                                },
                                                null,
                                                null,
                                                null,
                                                CommandFlags.Buffered,
                                                cancellationToken);

            await sqlConnection.ExecuteScalarAsync(command);
        }
    }
}
