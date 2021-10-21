using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Models.Internal;
using Datack.Agent.Services.DataConnections;
using Datack.Common.Models.Internal;
using StringTokenFormatter;

namespace Datack.Agent.Services
{
    public class DatabaseAdapter
    {
        private readonly SqlServerConnection _sqlServerConnection;
        private readonly DataProtector _dataProtector;

        public DatabaseAdapter(SqlServerConnection sqlServerConnection, DataProtector dataProtector)
        {
            _sqlServerConnection = sqlServerConnection;
            _dataProtector = dataProtector;
        }

        public String CreateConnectionString(String connectionString, String password, Boolean decryptPassword)
        {
            if (decryptPassword && !String.IsNullOrWhiteSpace(password))
            {
                password = _dataProtector.Decrypt(password);
            }

            return connectionString.FormatToken("password", password ?? "");
        }

        public async Task<String> TestConnection(String connectionString, CancellationToken cancellationToken)
        {
            try
            {
                await _sqlServerConnection.Test(connectionString, cancellationToken);
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<IList<Database>> GetDatabaseList(String connectionString, CancellationToken cancellationToken)
        {
            return await _sqlServerConnection.GetDatabaseList(connectionString, cancellationToken);
        }

        public async Task CreateBackup(String connectionString, String databaseName, String destinationFilePath, Action<DatabaseProgressEvent> progressCallback, CancellationToken cancellationToken)
        {
            await _sqlServerConnection.CreateBackup(connectionString, databaseName, destinationFilePath, progressCallback, cancellationToken);
        }
    }
}
