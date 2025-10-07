using Datack.Agent.Models.Internal;
using Datack.Agent.Services.DataConnections;
using Datack.Common.Models.Internal;
using StringTokenFormatter;

namespace Datack.Agent.Services;

public class DatabaseAdapter(SqlServerConnection sqlServerConnection, PostgresConnection postgresConnection, DataProtector dataProtector)
{
    public String CreateConnectionString(String connectionString, String? password, Boolean decryptPassword)
    {
        if (decryptPassword && !String.IsNullOrWhiteSpace(password))
        {
            password = dataProtector.Decrypt(password);
        }

        return connectionString.FormatFromObject(new
        {
            password = password ?? ""
        });
    }

    public async Task<String> TestConnection(String databaseType, String connectionString, CancellationToken cancellationToken)
    {
        try
        {
            await GetConnection(databaseType).Test(connectionString, cancellationToken);

            return "Success";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public async Task<IList<Database>> GetDatabaseList(String databaseType, String connectionString, CancellationToken cancellationToken)
    {
        return await GetConnection(databaseType).GetDatabaseList(connectionString, cancellationToken);
    }

    public async Task CreateBackup(String databaseType,
                                   String connectionString,
                                   String databaseName,
                                   String? backupType,
                                   String? password,
                                   String? options,
                                   String destinationFilePath,
                                   Action<DatabaseProgressEvent> progressCallback,
                                   CancellationToken cancellationToken)
    {
        await GetConnection(databaseType).CreateBackup(connectionString, databaseName, backupType, password, options, destinationFilePath, progressCallback, cancellationToken);
    }

    public async Task RestoreBackup(String databaseType,
                                    String connectionString,
                                    String databaseName,
                                    String? databaseLocation,
                                    String? password,
                                    String? options,
                                    String sourceFilePath,
                                    Action<DatabaseProgressEvent> progressCallback,
                                    CancellationToken cancellationToken)
    {
        await GetConnection(databaseType).RestoreBackup(connectionString, databaseName, databaseLocation, password, options, sourceFilePath, progressCallback, cancellationToken);
    }

    private IDatabaseConnection GetConnection(String? databaseType)
    {
        if (String.IsNullOrWhiteSpace(databaseType))
        {
            throw new("Invalid Database Type");
        }

        if (databaseType == "sqlServer")
        {
            return sqlServerConnection;
        }

        if (databaseType == "postgreSql")
        {
            return postgresConnection;
        }

        throw new($"Invalid database type {databaseType}");
    }
}