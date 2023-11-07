using CliWrap;
using CliWrap.EventStream;
using Datack.Agent.Models.Internal;
using Datack.Common.Models.Internal;
using Npgsql;
using System.Text;
using System.Text.RegularExpressions;

namespace Datack.Agent.Services.DataConnections;

public class PostgresConnection : IDatabaseConnection
{
    public async Task Test(String connectionString, CancellationToken cancellationToken)
    {
        await using var sqlConnection = NpgsqlDataSource.Create(connectionString);

        await sqlConnection.OpenConnectionAsync(cancellationToken);
    }

    public async Task<IList<Database>> GetDatabaseList(String connectionString, CancellationToken cancellationToken)
    {
        var result = new List<Database>();

        await using var sqlConnection = NpgsqlDataSource.Create(connectionString);

        await sqlConnection.OpenConnectionAsync(cancellationToken);

        await using var command = sqlConnection.CreateCommand("SELECT oid::integer, datname, true FROM pg_database WHERE datistemplate = false");
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new Database
            {
                DatabaseId = reader.GetInt32(0),
                DatabaseName = reader.GetString(1),
                HasAccess = reader.GetBoolean(2),
                HasFullbackup = true
            });
        }

        var databaseList = result
                           .OrderBy(m => m.DatabaseName == "postgres" ? "" : m.DatabaseName)
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
        if (backupType != "Full")
        {
            throw new Exception($"Unsupported backup type {backupType}, only Full backups are supported for Postgresql");
        }

        var server = Get(connectionString, "Server");
        var userName = Get(connectionString, "Username");

        var args = new List<String>
        {
            "--verbose",
            $"--dbname=postgresql://{userName}:{password}@{server}/{databaseName}",
            $@"--file ""{destinationFilePath}"""
        };

        if (!String.IsNullOrWhiteSpace(options))
        {
            args.Add(options);
        }
        else
        {
            args.Add("--format c");
            args.Add("-Z 5");
        }

        var executable = $"{AppDomain.CurrentDomain.BaseDirectory}Pgsql{Path.DirectorySeparatorChar}pg_dump.exe";

        var logCmd = @$"""{executable}"" {String.Join(" ", args)}";
                
        if (!String.IsNullOrWhiteSpace(password))
        {
            logCmd = logCmd.Replace($":{password}@", ":****@");
        }

        progressCallback.Invoke(new DatabaseProgressEvent
        {
            Message = $"Starting backup {Environment.NewLine}{logCmd}",
            Source = "Datack"
        });

        var stdErrBuffer = new StringBuilder();

        try
        {
            var cmd = Cli.Wrap(executable).WithArguments(args, false);

            await foreach (var cmdEvent in cmd.ListenAsync(cancellationToken))
            {
                switch (cmdEvent)
                {
                    case StandardOutputCommandEvent stdOut:
                    {
                        progressCallback.Invoke(new DatabaseProgressEvent
                        {
                            Message = stdOut.Text,
                            Source = "Datack"
                        });

                        break;
                    }
                    case StandardErrorCommandEvent stdErr:
                    {
                        progressCallback.Invoke(new DatabaseProgressEvent
                        {
                            Message = stdErr.Text,
                            Source = "Datack"
                        });

                        stdErrBuffer.AppendLine(stdErr.Text);

                        break;
                    }
                }
            }
        }
        catch
        {
            throw new Exception(stdErrBuffer.ToString());
        }
    }

    private static String Get(String connectionString, String key)
    {
        var match = Regex.Match(connectionString, $"{key}=(.*?)[;^]", RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            throw new Exception($"Cannot find the parameter {key} in the connection string");
        }

        return match.Groups[1].Captures[0].Value;
    }
}