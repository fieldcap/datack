using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Models.Internal;
using Datack.Agent.Services.DataConnections;
using Datack.Common.Models.Internal;

namespace Datack.Agent.Services
{
    public class DatabaseAdapter
    {
        private readonly SqlServerConnection _sqlServerConnection;

        public DatabaseAdapter(SqlServerConnection sqlServerConnection)
        {
            _sqlServerConnection = sqlServerConnection;
        }

        public async Task<String> TestConnection(ServerDbSettings serverDbSettings, CancellationToken cancellationToken)
        {
            try
            {
                await _sqlServerConnection.Test(serverDbSettings, cancellationToken);
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        
        public async Task<IList<Database>> GetDatabaseList(CancellationToken cancellationToken)
        {
            return await _sqlServerConnection.GetDatabaseList(cancellationToken);
        }

        public async Task<IList<File>> GetFileList(CancellationToken cancellationToken)
        {
            return await _sqlServerConnection.GetFileList(cancellationToken);
        }

        public async Task CreateBackup(String databaseName, String destinationFilePath, Action<DatabaseProgressEvent> progressCallback, CancellationToken cancellationToken)
        {
            await _sqlServerConnection.CreateBackup(databaseName, destinationFilePath, progressCallback, cancellationToken);
        }
    }
}
