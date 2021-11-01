using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Datack.Agent.Models.Internal;
using Datack.Common.Models.Internal;
using Microsoft.Data.SqlClient;
using StringTokenFormatter;

namespace Datack.Agent.Services.DataConnections
{
    public class SqlServerConnection
    {
        public async Task Test(String connectionString, CancellationToken cancellationToken)
        {
            await using var sqlConnection = new SqlConnection(connectionString);

            await sqlConnection.OpenAsync(cancellationToken);
        }

        public async Task<IList<Database>> GetDatabaseList(String connectionString, CancellationToken cancellationToken)
        {
            await using var sqlConnection = new SqlConnection(connectionString);

            await sqlConnection.OpenAsync(cancellationToken);

            var result = await sqlConnection.QueryAsync<Database>(@"SELECT 
    database_id AS 'DatabaseId',
	name AS 'DatabaseName', 
	HAS_PERMS_BY_NAME(name, 'DATABASE', 'BACKUP DATABASE') AS 'HasAccess' 
FROM 
	sys.databases");

            var databaseList = result
                               .OrderBy(m => m.DatabaseId > 4 ? m.DatabaseName : "")
                               .ToList();

            return databaseList;
        }

        public async Task CreateBackup(String connectionString, String databaseName, String backupType, String options, String destinationFilePath, Action<DatabaseProgressEvent> progressCallback, CancellationToken cancellationToken)
        {
            await using var sqlConnection = new SqlConnection(connectionString);

            if (String.IsNullOrWhiteSpace(options))
            {
                options = $"NAME = N'{{ItemName}} {{BackupType}} Backup', SKIP, STATS = 10";
            }

            options = options.FormatToken(new
            {
                ItemName = databaseName,
                BackupType = backupType
            });

            sqlConnection.InfoMessage += (_, args) =>
            {
                if (String.IsNullOrWhiteSpace(args.Message))
                {
                    return;
                }

                progressCallback?.Invoke(new DatabaseProgressEvent
                {
                    Message = args.Message,
                    Source = args.Source
                });
            };

            var queryHeader = backupType switch
            {
                "Full" => $@"BACKUP DATABASE @DatabaseName TO DISK = @FilePath WITH",
                "Differential" => $@"BACKUP DATABASE @DatabaseName TO DISK = @FilePath WITH DIFFERENTIAL,",
                "TransactionLog" => $@"BACKUP LOG @DatabaseName TO DISK = @FilePath WITH",
                _ => throw new Exception($"Unknown backup type {backupType}")
            };

            var query = $"{queryHeader} {options}";

            progressCallback?.Invoke(new DatabaseProgressEvent
            {
                Message = $"Starting backup script{Environment.NewLine}{query}",
                Source = "Datack"
            });

            var command = new CommandDefinition(query,
                                                new
                                                {
                                                    DatabaseName = databaseName,
                                                    FilePath = destinationFilePath
                                                },
                                                null,
                                                Int32.MaxValue,
                                                null,
                                                CommandFlags.Buffered,
                                                cancellationToken);
            
            await sqlConnection.ExecuteScalarAsync(command);
        }
    }
}
