using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<String> TestConnection(ServerDbSettings serverDbSettings)
        {
            try
            {
                await _sqlServerConnection.Test(serverDbSettings);
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        
        public async Task<IList<Database>> GetDatabaseList()
        {
            return await _sqlServerConnection.GetDatabaseList();
        }

        public async Task<IList<File>> GetFileList()
        {
            return await _sqlServerConnection.GetFileList();
        }
    }
}
