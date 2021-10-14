using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Datack.Agent.Models.Internal;
using Datack.Common.Models.Internal;

namespace Datack.Agent.Services.DataConnections
{
    public class SqlServerConnection
    {
        private readonly Servers _servers;

        public SqlServerConnection(Servers servers)
        {
            _servers = servers;
        }

        private async Task<String> GetConnectionString()
        {
            var server = await _servers.GetServer();
            var dbSettings = server.DbSettings;

#if DEBUG
            dbSettings.ConnectionTimeout = 1000;
#endif

            return BuildConnectionString(dbSettings);
    }
        

        private static String BuildConnectionString(ServerDbSettings dbSettings)
        {
            return $"Server={dbSettings.Server};User Id={dbSettings.UserName};Password={dbSettings.Password};Timeout={dbSettings.ConnectionTimeout}";
        }
        
        public async Task Test(ServerDbSettings serverDbSettings, CancellationToken cancellationToken)
        {
            String connectionString;
            if (serverDbSettings == null)
            {
                connectionString = await GetConnectionString();
            }
            else
            {
                connectionString = BuildConnectionString(serverDbSettings);
            }

            await using var sqlConnection = new SqlConnection(connectionString);

            await sqlConnection.OpenAsync(cancellationToken);
        }

        public async Task<IList<Database>> GetDatabaseList(CancellationToken cancellationToken)
        {
            await using var sqlConnection = new SqlConnection(await GetConnectionString());

            await sqlConnection.OpenAsync(cancellationToken);

            var result = await sqlConnection.QueryAsync<Database>(@"SELECT 
	name AS 'DatabaseName', 
	HAS_PERMS_BY_NAME(name, 'DATABASE', 'BACKUP DATABASE') AS 'HasAccess' 
FROM 
	sys.databases");

            return result.ToList();
        }

        public async Task<IList<File>> GetFileList(CancellationToken cancellationToken)
        {
            await using var sqlConnection = new SqlConnection(await GetConnectionString());

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

        public async Task CreateBackup(String databaseName, String destinationFilePath, Action<DatabaseProgressEvent> progressCallback, CancellationToken cancellationToken)
        {
            await using var sqlConnection = new SqlConnection(await GetConnectionString());

            var backupName = $"{databaseName} Backup";

            sqlConnection.InfoMessage += (_, args) =>
            {
                progressCallback?.Invoke(new DatabaseProgressEvent
                {
                    Message = args.Message,
                    Source = args.Source
                });
            };

            // Use ExecuteScalarAsync otherwise the InfoMessage event only fires when all messages are processed.
            await sqlConnection.ExecuteScalarAsync<Int32>($@"BACKUP DATABASE @DatabaseName 
TO DISK = @FilePath WITH NOFORMAT, 
INIT,
NAME = @BackupName, 
SKIP, 
NOREWIND, 
NOUNLOAD,
STATS = 10", new
            {
                DatabaseName = databaseName,
                FilePath = destinationFilePath,
                BackupName = backupName
            });
        }
    }
}
