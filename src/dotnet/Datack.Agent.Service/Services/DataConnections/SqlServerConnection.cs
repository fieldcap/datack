using Dapper;
using Datack.Agent.Models.Internal;
using Datack.Common.Models.Internal;
using Microsoft.Data.SqlClient;
using StringTokenFormatter;

namespace Datack.Agent.Services.DataConnections;

public class SqlServerConnection : IDatabaseConnection
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
	HAS_PERMS_BY_NAME(name, 'DATABASE', 'BACKUP DATABASE') AS 'HasAccess',
	(SELECT IIF(COUNT(*) = 0, 0, 1) FROM msdb.dbo.backupset WHERE type = 'D' AND database_name = sys.databases.name AND is_copy_only = 0) AS 'HasFullbackup'
FROM 
	sys.databases");

        var databaseList = result
                           .OrderBy(m => m.DatabaseId > 4 ? m.DatabaseName : "")
                           .ToList();

        return databaseList;
    }

    public async Task CreateBackup(String connectionString, 
                                   String databaseName, 
                                   String? backupType, 
                                   String? password, 
                                   String? options, 
                                   String destinationFilePath, 
                                   Action<DatabaseProgressEvent> progressCallback, 
                                   CancellationToken cancellationToken)
    {
        await using var sqlConnection = new SqlConnection(connectionString);

        if (backupType == null)
        {
            throw new Exception("Backup type cannot be null");
        }

        if (backupType != "Full" && backupType != "Differential" && backupType != "TransactionLog")
        {
            throw new Exception($"Invalid backup type {backupType}, has to be one of Full, Differential or TransactionLog");
        }

        if (String.IsNullOrWhiteSpace(options))
        {
            options = $"NAME = N'{{ItemName}} {{BackupType}} Backup', SKIP, STATS = 10";
        }

        options = options.FormatFromObject(new
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

            progressCallback.Invoke(new DatabaseProgressEvent
            {
                Message = args.Message,
                Source = args.Source
            });
        };

        var queryHeader = backupType switch
        {
            "Full" => "BACKUP DATABASE @DatabaseName TO DISK = @FilePath WITH",
            "Differential" => "BACKUP DATABASE @DatabaseName TO DISK = @FilePath WITH DIFFERENTIAL,",
            "TransactionLog" => "BACKUP LOG @DatabaseName TO DISK = @FilePath WITH",
            _ => throw new Exception($"Unknown backup type {backupType}")
        };

        var query = $"{queryHeader} {options}";

        progressCallback.Invoke(new DatabaseProgressEvent
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

    public async Task RestoreBackup(String connectionString,
                                    String databaseName,
                                    String? password,
                                    String? options,
                                    String sourceFilePath,
                                    Action<DatabaseProgressEvent> progressCallback,
                                    CancellationToken cancellationToken)
    {
        await using var sqlConnection = new SqlConnection(connectionString);

        if (String.IsNullOrWhiteSpace(options))
        {
            options = "STATS = 5";
        }

        sqlConnection.InfoMessage += (_, args) =>
        {
            if (String.IsNullOrWhiteSpace(args.Message))
            {
                return;
            }

            progressCallback.Invoke(new DatabaseProgressEvent
            {
                Message = args.Message,
                Source = args.Source
            });
        };

        var logicalNameQuery = $@"
DECLARE @Table TABLE (
    LogicalName varchar(128),
    [PhysicalName] varchar(128), 
    [Type] varchar, 
    [FileGroupName] varchar(128), 
    [Size] varchar(128),
    [MaxSize] varchar(128), 
    [FileId]varchar(128), 
    [CreateLSN]varchar(128), 
    [DropLSN]varchar(128), 
    [UniqueId]varchar(128), 
    [ReadOnlyLSN]varchar(128), 
    [ReadWriteLSN]varchar(128),
    [BackupSizeInBytes]varchar(128), 
    [SourceBlockSize]varchar(128), 
    [FileGroupId]varchar(128), 
    [LogGroupGUID]varchar(128), 
    [DifferentialBaseLSN]varchar(128), 
    [DifferentialBaseGUID]varchar(128), 
    [IsReadOnly]varchar(128), 
    [IsPresent]varchar(128), 
    [TDEThumbprint]varchar(128),
    [SnapshotUrl]varchar(128)
)
DECLARE @Path varchar(1000)='{sourceFilePath}'
DECLARE @LogicalNameData varchar(128),@LogicalNameLog varchar(128)
INSERT INTO @table
EXEC('
RESTORE FILELISTONLY
   FROM DISK=''' +@Path+ '''
   ')

   SET @LogicalNameData=(SELECT LogicalName FROM @Table WHERE Type = 'D')
   SET @LogicalNameLog=(SELECT LogicalName FROM @Table WHERE Type = 'L')

SELECT @LogicalNameData,@LogicalNameLog";

        var logicalNamesCommand = new CommandDefinition(logicalNameQuery,
                                                        null,
                                                        null,
                                                        Int32.MaxValue,
                                                        null,
                                                        CommandFlags.Buffered,
                                                        cancellationToken);

        var logicalNamesQueryDataReader = await sqlConnection.ExecuteReaderAsync(logicalNamesCommand);

        var logicalNamesQueryDataRow = await logicalNamesQueryDataReader.ReadAsync(cancellationToken);

        if (!logicalNamesQueryDataRow)
        {
            throw new Exception($"Unable to determine the logical file name of the backup set.{Environment.NewLine}{logicalNameQuery}");
        }

        var logicalNameData = logicalNamesQueryDataReader.GetFieldValue<String>(0);
        var logicalNameLog = logicalNamesQueryDataReader.GetFieldValue<String>(1);

        var killQuery = $@"DECLARE @kill varchar(8000) = '';  
SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), session_id) + ';'  
FROM sys.dm_exec_sessions
WHERE database_id  = db_id('{databaseName}') or login_name = '{databaseName}'

EXEC(@kill);";

        progressCallback.Invoke(new DatabaseProgressEvent
        {
            Message = $"Killing connections to target database{Environment.NewLine}{killQuery}",
            Source = "Datack"
        });

        var killCommand = new CommandDefinition(killQuery,
                                                null,
                                                null,
                                                Int32.MaxValue,
                                                null,
                                                CommandFlags.Buffered,
                                                cancellationToken);
            
        await sqlConnection.ExecuteScalarAsync(killCommand);

        var queryHeader = "RESTORE DATABASE @DatabaseName FROM DISK = @FilePath WITH FILE = 1,";

        var query = $"{queryHeader} {options}";

        query = query.FormatFromObject(new
        {
            DatabaseName = databaseName,
            FilePath = sourceFilePath,
            LogicalNameData = logicalNameData,
            LogicalNameLog = logicalNameLog
        });

        progressCallback.Invoke(new DatabaseProgressEvent
        {
            Message = $"Starting restore script{Environment.NewLine}{query}",
            Source = "Datack"
        });

        var command = new CommandDefinition(query,
                                            new
                                            {
                                                DatabaseName = databaseName,
                                                FilePath = sourceFilePath,
                                                LogicalNameData = logicalNameData,
                                                LogicalNameLog = logicalNameLog
                                            },
                                            null,
                                            Int32.MaxValue,
                                            null,
                                            CommandFlags.Buffered,
                                            cancellationToken);
            
        await sqlConnection.ExecuteScalarAsync(command);
    }
}